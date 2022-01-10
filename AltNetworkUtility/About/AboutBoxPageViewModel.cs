using System;
using System.Windows.Input;

using AltNetworkUtility.Services;

using Microsoft.Toolkit.Mvvm.Input;

using Serilog;

namespace AltNetworkUtility.About
{
    public class AboutBoxPageViewModel : ViewModelBase
	{
        readonly ILogger Log = Serilog.Log.ForContext<AboutBoxPageViewModel>();

        public ICommand OpenUrlCommand { get; }

        public AboutBoxPageViewModel()
		{
			OpenUrlCommand = new RelayCommand<string>(urlString =>
            {
                if (!Uri.TryCreate(urlString, UriKind.Absolute, out var url))
                {
                    Log.Warning("Could not create URL from {urlString}", urlString);
                    return;
                }

                Xamarin.Forms.DependencyService.Get<IOpenUrlService>().Open(url);
            });
		}
	}
}

