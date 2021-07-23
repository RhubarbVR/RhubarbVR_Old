/*
 * RhubarbVRApi
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = Org.OpenAPITools.Client.OpenAPIDateConverter;

namespace Org.OpenAPITools.Model
{
    /// <summary>
    /// StatusUpdate
    /// </summary>
    [DataContract(Name = "StatusUpdate")]
    public partial class StatusUpdate : IEquatable<StatusUpdate>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusUpdate" /> class.
        /// </summary>
        /// <param name="onlinelevel">onlinelevel.</param>
        /// <param name="customstatus">customstatus.</param>
        /// <param name="focusedsession">focusedsession.</param>
        /// <param name="outputdevice">outputdevice.</param>
        /// <param name="version">version.</param>
        /// <param name="ismobile">ismobile.</param>
        /// <param name="versionkey">versionkey.</param>
        public StatusUpdate(int? onlinelevel = default(int?), string customstatus = default(string), string focusedsession = default(string), string outputdevice = default(string), string version = default(string), bool? ismobile = default(bool?), string versionkey = default(string))
        {
            this.Onlinelevel = onlinelevel;
            this.Customstatus = customstatus;
            this.Focusedsession = focusedsession;
            this.Outputdevice = outputdevice;
            this.Version = version;
            this.Ismobile = ismobile;
            this.Versionkey = versionkey;
        }

        /// <summary>
        /// Gets or Sets Onlinelevel
        /// </summary>
        [DataMember(Name = "onlinelevel", EmitDefaultValue = true)]
        public int? Onlinelevel { get; set; }

        /// <summary>
        /// Gets or Sets Customstatus
        /// </summary>
        [DataMember(Name = "customstatus", EmitDefaultValue = true)]
        public string Customstatus { get; set; }

        /// <summary>
        /// Gets or Sets Focusedsession
        /// </summary>
        [DataMember(Name = "focusedsession", EmitDefaultValue = true)]
        public string Focusedsession { get; set; }

        /// <summary>
        /// Gets or Sets Outputdevice
        /// </summary>
        [DataMember(Name = "outputdevice", EmitDefaultValue = true)]
        public string Outputdevice { get; set; }

        /// <summary>
        /// Gets or Sets Version
        /// </summary>
        [DataMember(Name = "version", EmitDefaultValue = true)]
        public string Version { get; set; }

        /// <summary>
        /// Gets or Sets Ismobile
        /// </summary>
        [DataMember(Name = "ismobile", EmitDefaultValue = true)]
        public bool? Ismobile { get; set; }

        /// <summary>
        /// Gets or Sets Versionkey
        /// </summary>
        [DataMember(Name = "versionkey", EmitDefaultValue = true)]
        public string Versionkey { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StatusUpdate {\n");
            sb.Append("  Onlinelevel: ").Append(Onlinelevel).Append("\n");
            sb.Append("  Customstatus: ").Append(Customstatus).Append("\n");
            sb.Append("  Focusedsession: ").Append(Focusedsession).Append("\n");
            sb.Append("  Outputdevice: ").Append(Outputdevice).Append("\n");
            sb.Append("  Version: ").Append(Version).Append("\n");
            sb.Append("  Ismobile: ").Append(Ismobile).Append("\n");
            sb.Append("  Versionkey: ").Append(Versionkey).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as StatusUpdate);
        }

        /// <summary>
        /// Returns true if StatusUpdate instances are equal
        /// </summary>
        /// <param name="input">Instance of StatusUpdate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StatusUpdate input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Onlinelevel == input.Onlinelevel ||
                    (this.Onlinelevel != null &&
                    this.Onlinelevel.Equals(input.Onlinelevel))
                ) && 
                (
                    this.Customstatus == input.Customstatus ||
                    (this.Customstatus != null &&
                    this.Customstatus.Equals(input.Customstatus))
                ) && 
                (
                    this.Focusedsession == input.Focusedsession ||
                    (this.Focusedsession != null &&
                    this.Focusedsession.Equals(input.Focusedsession))
                ) && 
                (
                    this.Outputdevice == input.Outputdevice ||
                    (this.Outputdevice != null &&
                    this.Outputdevice.Equals(input.Outputdevice))
                ) && 
                (
                    this.Version == input.Version ||
                    (this.Version != null &&
                    this.Version.Equals(input.Version))
                ) && 
                (
                    this.Ismobile == input.Ismobile ||
                    (this.Ismobile != null &&
                    this.Ismobile.Equals(input.Ismobile))
                ) && 
                (
                    this.Versionkey == input.Versionkey ||
                    (this.Versionkey != null &&
                    this.Versionkey.Equals(input.Versionkey))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Onlinelevel != null)
                    hashCode = hashCode * 59 + this.Onlinelevel.GetHashCode();
                if (this.Customstatus != null)
                    hashCode = hashCode * 59 + this.Customstatus.GetHashCode();
                if (this.Focusedsession != null)
                    hashCode = hashCode * 59 + this.Focusedsession.GetHashCode();
                if (this.Outputdevice != null)
                    hashCode = hashCode * 59 + this.Outputdevice.GetHashCode();
                if (this.Version != null)
                    hashCode = hashCode * 59 + this.Version.GetHashCode();
                if (this.Ismobile != null)
                    hashCode = hashCode * 59 + this.Ismobile.GetHashCode();
                if (this.Versionkey != null)
                    hashCode = hashCode * 59 + this.Versionkey.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}