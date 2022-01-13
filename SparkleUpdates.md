(This is internal developer documentation.)

ANU uses [**SparkleSharp**](https://github.com/rainycape/SparkleSharp/tree/master/Sparkle),
which wraps Sparkle for C#. It works, but is old, and therefore uses some older
Sparkle tooling. Specifically, we can't use stuff like `SUPublicEDKey`, because
this old version doesn't understand it. Instead, we use `SUPublicDSAKey`, which
unfortunately the current tooling no longer supports. Oops.

You need the [Sparkle 1.20 tools](https://github.com/sparkle-project/Sparkle/releases/tag/1.20.0)
to make a new release.

* In `Info.plist`, **Bundle version** needs to be machine-comparable, e.g.
`1.0.7`. **Bundle versions string (short)** is human-readable and will be shown
in the update dialog. Increment both.
* Make a release build, and zip the app bundle. Give the archive a name that
follows the convention, e.g. `Alt Network Utility-1.0b7.zip`. Put it next to
other releases in a folder we'll call… `releases`.
* Optionally, also make a HTML file, and name it the same, e.g.
`Alt Network Utility-1.0b7.html`. This can be partial HTML if short.
* From the tools, run something like `./generate_appcast -f dsa_priv.pem ../releases`.
This generates an `appcast.xml`, but isn't quite right in multiple ways.
* Call something like `openssl dgst -sha1 -binary < Alt Network Utility-1.0b7.zip | openssl dgst -dss1 -sign dsa_priv.pem | base64`.
(Other docs say to use `openssl enc -base64` instead, but this doesn't look right
to me.) Take the new `item` node `generate_appcast` has made and place it in the
*original* (correct) `appcast.xml`. Add an attribute to the `enclosure` tag:
`sparkle:dsaSignature="your base64 stuff"`.
* We're still not done. You also need to change the `enclosure`'s `url` attribute
to match `https://github.com/chucker/AltNetworkUtility/releases/download/…`.
* Push this entire `appcast.xml` into the `main` branch. After a few minutes,
`curl https://chucker.github.io/AltNetworkUtility/appcast.xml` should reflect
your change.
* Upload the zip archive to GitHub as a release.

If we ever move to a newer Sparkle, the custom invocation of `openssl` and the
manual adding of the `signature` attribute won't be necessary any more.
