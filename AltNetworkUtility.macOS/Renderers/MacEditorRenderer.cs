using System;
using System.ComponentModel;
using System.Reflection;

using AltNetworkUtility.macOS.Renderers;

using AppKit;

using CoreGraphics;

using Foundation;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.MacOS;

#nullable enable

[assembly: ExportRenderer(typeof(Editor), typeof(MacOSEditorRenderer))]
namespace AltNetworkUtility.macOS.Renderers
{
    public class MacOSEditorRenderer : ViewRenderer<Editor, NSScrollView>
    {
        Serilog.ILogger Log = Serilog.Log.ForContext<MacOSEditorRenderer>();

        const string NewLineSelector = "insertNewline";
        bool _disposed;
        CGSize _previousSize;

        NSTextView? _nativeEditor;

        IEditorController ElementController => Element;

        public override void Layout()
        {
            base.Layout();

            if (Element != null && _previousSize != Bounds.Size)
                SetBackground(Element.Background);
            
            _previousSize = Bounds.Size;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var scroller = new NSScrollView
                {
                    AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
                    BorderType = NSBorderType.NoBorder,
                    DrawsBackground = false,
                    HasHorizontalScroller = false,
                    HasVerticalScroller = true
                };

                _nativeEditor = new NSTextView(new CGRect(new CGPoint(0, 0), scroller.ContentSize))
                {
                    AutoresizingMask = NSViewResizingMask.WidthSizable,
                    HorizontallyResizable = false,
                    MinSize = new CGSize(0, scroller.ContentSize.Height),
                    MaxSize = new CGSize(float.MaxValue, float.MaxValue),
                    VerticallyResizable = true
                };

                scroller.DocumentView = _nativeEditor;

                SetNativeControl(scroller);

                _nativeEditor.TextDidChange += HandleChanged;
                _nativeEditor.TextDidBeginEditing += OnEditingBegan;
                _nativeEditor.TextDidEndEditing += OnEditingEnded;
                _nativeEditor.DoCommandBySelector = (textView, commandSelector) =>
                {
                    var result = false;
                    if (commandSelector.Name.StartsWith(NewLineSelector, StringComparison.InvariantCultureIgnoreCase))
                    {
                        textView.InsertText(new NSString(Environment.NewLine));
                        result = true;
                    }
                    return result;
                };
            }

            if (e.NewElement == null) return;
            UpdateText();
            UpdateFont();
            UpdateTextColor();
            UpdateEditable();
            UpdateMaxLength();
            UpdateIsReadOnly();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Editor.TextProperty.PropertyName ||
                e.PropertyName == Editor.TextTransformProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
                UpdateEditable();
            else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
                UpdateTextColor();
            else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
                UpdateFont();
            else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
                UpdateMaxLength();
            else if (e.PropertyName == Xamarin.Forms.InputView.IsReadOnlyProperty.PropertyName)
                UpdateIsReadOnly();
        }

        protected override void SetBackgroundColor(Color color)
        {
            if (Control == null || _nativeEditor == null)
                return;

            Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

            if (color == Color.Transparent)
            {
                Control.DrawsBackground = false;
                _nativeEditor.DrawsBackground = false;
            }
            else
            {
                Control.DrawsBackground = true;
                _nativeEditor.DrawsBackground = true;
            }

            base.SetBackgroundColor(color);
        }

        protected override void SetBackground(Brush brush)
        {
            if (Control == null)
                return;

            var backgroundImage = this.GetBackgroundImage(brush);
            Control.BackgroundColor = backgroundImage != null ? NSColor.FromPatternImage(backgroundImage) : NSColor.Clear;

            base.SetBackground(brush);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                if (_nativeEditor != null)
                {
                    _nativeEditor.TextDidChange -= HandleChanged;
                    _nativeEditor.TextDidBeginEditing -= OnEditingBegan;
                    _nativeEditor.TextDidEndEditing -= OnEditingEnded;
                }
            }
            base.Dispose(disposing);
        }

        void HandleChanged(object sender, EventArgs e)
        {
            if (_nativeEditor == null)
                return;

            UpdateMaxLength();

            ElementController.SetValueFromRenderer(Editor.TextProperty, _nativeEditor.Value);
        }

        void OnEditingEnded(object sender, EventArgs eventArgs)
        {
            Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
            ElementController.SendCompleted();
        }

        void OnEditingBegan(object sender, EventArgs eventArgs)
        {
            ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
        }

        void UpdateEditable()
        {
            if (_nativeEditor == null)
                return;

            _nativeEditor.Editable = Element.IsEnabled;
        }

        void UpdateFont()
        {
            if (_nativeEditor == null)
                return;

            // the ToNSFont() overload we want is internal
            // https://github.com/xamarin/Xamarin.Forms/blob/caab66bcf9614aca0c0805d560a34e176d196e17/Xamarin.Forms.Platform.MacOS/Extensions/FontExtensions.cs#L13
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(FontExtensions).GetMethod("ToNSFont", bindingFlags, null,
                                                          new[] { typeof(IFontElement) }, null);

            if (method?.Invoke(null, new[] { Element }) is NSFont font)
                _nativeEditor.Font = font;
        }

        void UpdateText()
        {
            if (_nativeEditor == null)
                return;

            var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
            if (_nativeEditor.String != text)
                _nativeEditor.Value = text;
        }

        void UpdateTextColor()
        {
            if (_nativeEditor == null)
                return;

            var textColor = Element.TextColor;

            _nativeEditor.SetTextColor(textColor.IsDefault ? NSColor.Black : textColor.ToNSColor(),
                                       new NSRange(0, _nativeEditor.String.Length));
        }

        void UpdateMaxLength()
        {
            if (_nativeEditor == null)
                return;

            var currentControlText = _nativeEditor.String;

            if (currentControlText.Length > Element?.MaxLength)
                _nativeEditor.Value = currentControlText.Substring(0, Element.MaxLength);
        }

        void UpdateIsReadOnly()
        {
            if (_nativeEditor == null)
                return;

            _nativeEditor.Editable = !Element.IsReadOnly;
            if (Element.IsReadOnly && Control.Window?.FirstResponder == _nativeEditor)
                Control.Window?.MakeFirstResponder(null);
        }
    }
}
