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
	/// PublicSession
	/// </summary>
	[DataContract(Name = "PublicSession")]
	public partial class PublicSession : IEquatable<PublicSession>, IValidatableObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PublicSession" /> class.
		/// </summary>
		/// <param name="uuid">uuid.</param>
		/// <param name="name">name.</param>
		/// <param name="correspondingworlduuid">correspondingworlduuid.</param>
		/// <param name="normalizedname">normalizedname.</param>
		/// <param name="sessionusers">sessionusers.</param>
		/// <param name="tags">tags.</param>
		/// <param name="hostuuid">hostuuid.</param>
		/// <param name="hostusername">hostusername.</param>
		/// <param name="thumbnailurl">thumbnailurl.</param>
		/// <param name="versionkey">versionkey.</param>
		/// <param name="versionstring">versionstring.</param>
		/// <param name="sessionstype">sessionstype.</param>
		/// <param name="accesslevel">accesslevel.</param>
		/// <param name="eighteenandolder">eighteenandolder.</param>
		/// <param name="joinedusers">joinedusers.</param>
		/// <param name="activeUsers">activeUsers.</param>
		/// <param name="maxUsers">maxUsers.</param>
		/// <param name="mobilefriendly">mobilefriendly.</param>
		/// <param name="hasended">hasended.</param>
		/// <param name="lastupdate">lastupdate.</param>
		/// <param name="sessionbegintime">sessionbegintime.</param>
		public PublicSession(string uuid = default(string), string name = default(string), string correspondingworlduuid = default(string), string normalizedname = default(string), List<string> sessionusers = default(List<string>), List<string> tags = default(List<string>), string hostuuid = default(string), string hostusername = default(string), string thumbnailurl = default(string), string versionkey = default(string), string versionstring = default(string), int sessionstype = default(int), int accesslevel = default(int), bool eighteenandolder = default(bool), int joinedusers = default(int), int activeUsers = default(int), int maxUsers = default(int), bool mobilefriendly = default(bool), bool hasended = default(bool), DateTime lastupdate = default(DateTime), DateTime sessionbegintime = default(DateTime))
		{
			this.Uuid = uuid;
			this.Name = name;
			this.Correspondingworlduuid = correspondingworlduuid;
			this.Normalizedname = normalizedname;
			this.Sessionusers = sessionusers;
			this.Tags = tags;
			this.Hostuuid = hostuuid;
			this.Hostusername = hostusername;
			this.Thumbnailurl = thumbnailurl;
			this.Versionkey = versionkey;
			this.Versionstring = versionstring;
			this.Sessionstype = sessionstype;
			this.Accesslevel = accesslevel;
			this.Eighteenandolder = eighteenandolder;
			this.Joinedusers = joinedusers;
			this.ActiveUsers = activeUsers;
			this.MaxUsers = maxUsers;
			this.Mobilefriendly = mobilefriendly;
			this.Hasended = hasended;
			this.Lastupdate = lastupdate;
			this.Sessionbegintime = sessionbegintime;
		}

		/// <summary>
		/// Gets or Sets Uuid
		/// </summary>
		[DataMember(Name = "uuid", EmitDefaultValue = true)]
		public string Uuid { get; set; }

		/// <summary>
		/// Gets or Sets Name
		/// </summary>
		[DataMember(Name = "name", EmitDefaultValue = true)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets Correspondingworlduuid
		/// </summary>
		[DataMember(Name = "correspondingworlduuid", EmitDefaultValue = true)]
		public string Correspondingworlduuid { get; set; }

		/// <summary>
		/// Gets or Sets Normalizedname
		/// </summary>
		[DataMember(Name = "normalizedname", EmitDefaultValue = true)]
		public string Normalizedname { get; set; }

		/// <summary>
		/// Gets or Sets Sessionusers
		/// </summary>
		[DataMember(Name = "sessionusers", EmitDefaultValue = true)]
		public List<string> Sessionusers { get; set; }

		/// <summary>
		/// Gets or Sets Tags
		/// </summary>
		[DataMember(Name = "tags", EmitDefaultValue = true)]
		public List<string> Tags { get; set; }

		/// <summary>
		/// Gets or Sets Hostuuid
		/// </summary>
		[DataMember(Name = "hostuuid", EmitDefaultValue = true)]
		public string Hostuuid { get; set; }

		/// <summary>
		/// Gets or Sets Hostusername
		/// </summary>
		[DataMember(Name = "hostusername", EmitDefaultValue = true)]
		public string Hostusername { get; set; }

		/// <summary>
		/// Gets or Sets Thumbnailurl
		/// </summary>
		[DataMember(Name = "thumbnailurl", EmitDefaultValue = true)]
		public string Thumbnailurl { get; set; }

		/// <summary>
		/// Gets or Sets Versionkey
		/// </summary>
		[DataMember(Name = "versionkey", EmitDefaultValue = true)]
		public string Versionkey { get; set; }

		/// <summary>
		/// Gets or Sets Versionstring
		/// </summary>
		[DataMember(Name = "versionstring", EmitDefaultValue = true)]
		public string Versionstring { get; set; }

		/// <summary>
		/// Gets or Sets Sessionstype
		/// </summary>
		[DataMember(Name = "sessionstype", EmitDefaultValue = false)]
		public int Sessionstype { get; set; }

		/// <summary>
		/// Gets or Sets Accesslevel
		/// </summary>
		[DataMember(Name = "accesslevel", EmitDefaultValue = false)]
		public int Accesslevel { get; set; }

		/// <summary>
		/// Gets or Sets Eighteenandolder
		/// </summary>
		[DataMember(Name = "eighteenandolder", EmitDefaultValue = false)]
		public bool Eighteenandolder { get; set; }

		/// <summary>
		/// Gets or Sets Joinedusers
		/// </summary>
		[DataMember(Name = "joinedusers", EmitDefaultValue = false)]
		public int Joinedusers { get; set; }

		/// <summary>
		/// Gets or Sets ActiveUsers
		/// </summary>
		[DataMember(Name = "activeUsers", EmitDefaultValue = false)]
		public int ActiveUsers { get; set; }

		/// <summary>
		/// Gets or Sets MaxUsers
		/// </summary>
		[DataMember(Name = "maxUsers", EmitDefaultValue = false)]
		public int MaxUsers { get; set; }

		/// <summary>
		/// Gets or Sets Mobilefriendly
		/// </summary>
		[DataMember(Name = "mobilefriendly", EmitDefaultValue = false)]
		public bool Mobilefriendly { get; set; }

		/// <summary>
		/// Gets or Sets Hasended
		/// </summary>
		[DataMember(Name = "hasended", EmitDefaultValue = false)]
		public bool Hasended { get; set; }

		/// <summary>
		/// Gets or Sets Lastupdate
		/// </summary>
		[DataMember(Name = "lastupdate", EmitDefaultValue = false)]
		public DateTime Lastupdate { get; set; }

		/// <summary>
		/// Gets or Sets Sessionbegintime
		/// </summary>
		[DataMember(Name = "sessionbegintime", EmitDefaultValue = false)]
		public DateTime Sessionbegintime { get; set; }

		/// <summary>
		/// Returns the string presentation of the object
		/// </summary>
		/// <returns>String presentation of the object</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("class PublicSession {\n");
			sb.Append("  Uuid: ").Append(Uuid).Append("\n");
			sb.Append("  Name: ").Append(Name).Append("\n");
			sb.Append("  Correspondingworlduuid: ").Append(Correspondingworlduuid).Append("\n");
			sb.Append("  Normalizedname: ").Append(Normalizedname).Append("\n");
			sb.Append("  Sessionusers: ").Append(Sessionusers).Append("\n");
			sb.Append("  Tags: ").Append(Tags).Append("\n");
			sb.Append("  Hostuuid: ").Append(Hostuuid).Append("\n");
			sb.Append("  Hostusername: ").Append(Hostusername).Append("\n");
			sb.Append("  Thumbnailurl: ").Append(Thumbnailurl).Append("\n");
			sb.Append("  Versionkey: ").Append(Versionkey).Append("\n");
			sb.Append("  Versionstring: ").Append(Versionstring).Append("\n");
			sb.Append("  Sessionstype: ").Append(Sessionstype).Append("\n");
			sb.Append("  Accesslevel: ").Append(Accesslevel).Append("\n");
			sb.Append("  Eighteenandolder: ").Append(Eighteenandolder).Append("\n");
			sb.Append("  Joinedusers: ").Append(Joinedusers).Append("\n");
			sb.Append("  ActiveUsers: ").Append(ActiveUsers).Append("\n");
			sb.Append("  MaxUsers: ").Append(MaxUsers).Append("\n");
			sb.Append("  Mobilefriendly: ").Append(Mobilefriendly).Append("\n");
			sb.Append("  Hasended: ").Append(Hasended).Append("\n");
			sb.Append("  Lastupdate: ").Append(Lastupdate).Append("\n");
			sb.Append("  Sessionbegintime: ").Append(Sessionbegintime).Append("\n");
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
			return this.Equals(input as PublicSession);
		}

		/// <summary>
		/// Returns true if PublicSession instances are equal
		/// </summary>
		/// <param name="input">Instance of PublicSession to be compared</param>
		/// <returns>Boolean</returns>
		public bool Equals(PublicSession input)
		{
			if (input == null)
				return false;

			return
				(
					this.Uuid == input.Uuid ||
					(this.Uuid != null &&
					this.Uuid.Equals(input.Uuid))
				) &&
				(
					this.Name == input.Name ||
					(this.Name != null &&
					this.Name.Equals(input.Name))
				) &&
				(
					this.Correspondingworlduuid == input.Correspondingworlduuid ||
					(this.Correspondingworlduuid != null &&
					this.Correspondingworlduuid.Equals(input.Correspondingworlduuid))
				) &&
				(
					this.Normalizedname == input.Normalizedname ||
					(this.Normalizedname != null &&
					this.Normalizedname.Equals(input.Normalizedname))
				) &&
				(
					this.Sessionusers == input.Sessionusers ||
					this.Sessionusers != null &&
					input.Sessionusers != null &&
					this.Sessionusers.SequenceEqual(input.Sessionusers)
				) &&
				(
					this.Tags == input.Tags ||
					this.Tags != null &&
					input.Tags != null &&
					this.Tags.SequenceEqual(input.Tags)
				) &&
				(
					this.Hostuuid == input.Hostuuid ||
					(this.Hostuuid != null &&
					this.Hostuuid.Equals(input.Hostuuid))
				) &&
				(
					this.Hostusername == input.Hostusername ||
					(this.Hostusername != null &&
					this.Hostusername.Equals(input.Hostusername))
				) &&
				(
					this.Thumbnailurl == input.Thumbnailurl ||
					(this.Thumbnailurl != null &&
					this.Thumbnailurl.Equals(input.Thumbnailurl))
				) &&
				(
					this.Versionkey == input.Versionkey ||
					(this.Versionkey != null &&
					this.Versionkey.Equals(input.Versionkey))
				) &&
				(
					this.Versionstring == input.Versionstring ||
					(this.Versionstring != null &&
					this.Versionstring.Equals(input.Versionstring))
				) &&
				(
					this.Sessionstype == input.Sessionstype ||
					this.Sessionstype.Equals(input.Sessionstype)
				) &&
				(
					this.Accesslevel == input.Accesslevel ||
					this.Accesslevel.Equals(input.Accesslevel)
				) &&
				(
					this.Eighteenandolder == input.Eighteenandolder ||
					this.Eighteenandolder.Equals(input.Eighteenandolder)
				) &&
				(
					this.Joinedusers == input.Joinedusers ||
					this.Joinedusers.Equals(input.Joinedusers)
				) &&
				(
					this.ActiveUsers == input.ActiveUsers ||
					this.ActiveUsers.Equals(input.ActiveUsers)
				) &&
				(
					this.MaxUsers == input.MaxUsers ||
					this.MaxUsers.Equals(input.MaxUsers)
				) &&
				(
					this.Mobilefriendly == input.Mobilefriendly ||
					this.Mobilefriendly.Equals(input.Mobilefriendly)
				) &&
				(
					this.Hasended == input.Hasended ||
					this.Hasended.Equals(input.Hasended)
				) &&
				(
					this.Lastupdate == input.Lastupdate ||
					(this.Lastupdate != null &&
					this.Lastupdate.Equals(input.Lastupdate))
				) &&
				(
					this.Sessionbegintime == input.Sessionbegintime ||
					(this.Sessionbegintime != null &&
					this.Sessionbegintime.Equals(input.Sessionbegintime))
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
				if (this.Uuid != null)
					hashCode = hashCode * 59 + this.Uuid.GetHashCode();
				if (this.Name != null)
					hashCode = hashCode * 59 + this.Name.GetHashCode();
				if (this.Correspondingworlduuid != null)
					hashCode = hashCode * 59 + this.Correspondingworlduuid.GetHashCode();
				if (this.Normalizedname != null)
					hashCode = hashCode * 59 + this.Normalizedname.GetHashCode();
				if (this.Sessionusers != null)
					hashCode = hashCode * 59 + this.Sessionusers.GetHashCode();
				if (this.Tags != null)
					hashCode = hashCode * 59 + this.Tags.GetHashCode();
				if (this.Hostuuid != null)
					hashCode = hashCode * 59 + this.Hostuuid.GetHashCode();
				if (this.Hostusername != null)
					hashCode = hashCode * 59 + this.Hostusername.GetHashCode();
				if (this.Thumbnailurl != null)
					hashCode = hashCode * 59 + this.Thumbnailurl.GetHashCode();
				if (this.Versionkey != null)
					hashCode = hashCode * 59 + this.Versionkey.GetHashCode();
				if (this.Versionstring != null)
					hashCode = hashCode * 59 + this.Versionstring.GetHashCode();
				hashCode = hashCode * 59 + this.Sessionstype.GetHashCode();
				hashCode = hashCode * 59 + this.Accesslevel.GetHashCode();
				hashCode = hashCode * 59 + this.Eighteenandolder.GetHashCode();
				hashCode = hashCode * 59 + this.Joinedusers.GetHashCode();
				hashCode = hashCode * 59 + this.ActiveUsers.GetHashCode();
				hashCode = hashCode * 59 + this.MaxUsers.GetHashCode();
				hashCode = hashCode * 59 + this.Mobilefriendly.GetHashCode();
				hashCode = hashCode * 59 + this.Hasended.GetHashCode();
				if (this.Lastupdate != null)
					hashCode = hashCode * 59 + this.Lastupdate.GetHashCode();
				if (this.Sessionbegintime != null)
					hashCode = hashCode * 59 + this.Sessionbegintime.GetHashCode();
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
