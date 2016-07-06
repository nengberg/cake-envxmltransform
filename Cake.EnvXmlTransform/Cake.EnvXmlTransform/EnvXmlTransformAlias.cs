using System;

using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.EnvXmlTransform {
	/// <summary>
	/// Contains functionality for transformation configuration files for specific environments.
	/// </summary>
	/// /// <example>
	/// <code>
	///<![CDATA[
	/// Task("Apply-Config-Transformations")
	///   .Does(() => {
	///     var configFileFolder = "folder/**/*.config";
	///     var environment = "stage";
	///     ConfigTransform.ApplyTransformations(configFileFolder, environment);
	/// });
	///]]>
	/// </code>
	/// </example>

	[CakeAliasCategory("EnvXML")]
	public static class EnvXmlTransformAlias {
		[CakePropertyAlias]
		public static EnvXmlTransformRunner ConfigTransform(this ICakeContext context) {
			if(context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			return new EnvXmlTransformRunner(context);
		}
	}
}