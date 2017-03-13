# MSDeployProcessComparer

Simple tool to compare the output of an MSDeploy publish with web.config transforms vs SetParameters

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