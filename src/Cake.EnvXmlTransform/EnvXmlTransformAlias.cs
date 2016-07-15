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
	///	Task("Apply-Config-Transformations")
	///		.Does(() => {
	///			var configFileFolder = "folder/**/*.config";
	///			var environment = "stage";
	///			ConfigTransform.ApplyTransformations(configFileFolder, environment);
	/// });
	///]]>
	/// </code>
	/// </example>

	[CakeAliasCategory("Environmental XML configuration transformations")]
	public static class EnvXmlTransformAlias {
		[CakePropertyAlias]
		public static EnvironmentalXmlTransformRunner ConfigTransform(this ICakeContext context) {
			if(context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			return new EnvironmentalXmlTransformRunner(context);
		}
	}
}