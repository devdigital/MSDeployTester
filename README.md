# MSDeployTester

Simple utility application for testing MSDeploy packages and SetParameters

## Example

The following shows an example of performing a (real) `Any CPU` platform, `Release` configuration deploy to your local IIS Express server. This uses the `tfs` values provider, which must be present in a subfolder of the `plugins` directory. 

The `valueProviderOptions` is a JSON string of options to pass to the value provider. For the `tfs` values provider this includes the TFS URI, project name, release definition name, and release environment name. Note the use of apostrophes within the JSON string:

```
msdeploytester deploy -s C:\MyPath\MySolution.sln -p "Any CPU" -c "Release" 
--valueProviderId tfs --valueProviderOptions "{ 'uri': 'http://my/tfs/path', 'project': 'MyTFSProject', 
'release': 'My Release Definition', 'environment': 'My Release Definition Environment' }"
```

## Manually Editing IIS Express Sites

Use the `appcmd.exe` file within `C:\Program Files\IIS Express`. 

To list sites:

```
appcmd list site
```

To add a site:

```
appcmd add site /id:1 /name:xxx /bindings:http:/*:80:www.xxx.com /physicalPath:C:\Path
```

To delete a site:

```
appcmd delete site <sitename>
```
