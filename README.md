# Cake.EnvXmlTransform

This plugin enables you to transform configuration files based on the environment or machine you're deploying to.
An example could be for your production environment that have specific configuration files that you want to automatically transform.
This is an extension of [Cake.XdtTransform](https://github.com/phillipsj/Cake.XdtTransform) that allows you to use XdtTransforms directly. 

## Usage:

```csharp
#addin Cake.EnvXmlTransform

Task("Apply-Config-Transformations")
  .Does(() => {
    var configFileFolder = "folder/**/*.config";
    var environment = "production";
    ConfigTransform.ApplyTransformations(configFileFolder, environment);
});

```

That would transform your files named `file.production.config` into `file.config`.

This plugin is built to meet my own requirements out of my needs. 
Please, if you have any feature requests submit them as issues.  