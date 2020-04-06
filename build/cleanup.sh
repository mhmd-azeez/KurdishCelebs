# Remove the models and native libraries since they are very big and we already have them on the server
rm KurdishCelebs.WebApp/bin/x64/Release/netcoreapp3.1/linux-x64/publish/models -r
rm KurdishCelebs.WebApp/bin/x64/Release/netcoreapp3.1/linux-x64/publish/lib*.so
