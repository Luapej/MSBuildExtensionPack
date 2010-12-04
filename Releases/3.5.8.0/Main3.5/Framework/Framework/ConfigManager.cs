﻿//-----------------------------------------------------------------------
// <copyright file="ConfigManager.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Framework
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Configuration;
    using Microsoft.Build.Framework;

    public enum DotNetConfigurationFile
    {
        /// <summary>
        /// Update the machine.config.
        /// </summary>
        MachineConfig,

        /// <summary>
        /// Update the web.config in the framework config directory
        /// </summary>
        WebConfig
    }

    /// <summary>
    /// Task used to work with the .NET framework web.config and machine config files
    /// <b>Valid TaskActions are:</b>
    /// <para><i>RemoveAppSetting</i> (<b>Required: </b> SettingName <b>Optional: </b>Site, Path, ConfigurationFileType, SaveMode)</para>
    /// <para><i>RemoveConnectionString</i> (<b>Required: </b> SettingName <b>Optional: </b>Site, Path, ConfigurationFileType, SaveMode)</para>
    /// <para><i>SetAppSetting</i> (<b>Required: </b> SettingName <b>Optional: </b>Site, Path, SettingValue, ConfigurationFileType, SaveMode)</para>
    /// <para><i>SetConnectionString</i> (<b>Required: </b> SettingName <b>Optional: </b>Site, Path, SettingValue, ConfigurationFileType, SaveMode)</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="3.5" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///   <PropertyGroup>
    ///     <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///     <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///   </PropertyGroup>
    ///   <Import Project="$(TPath)"/>
    ///   <Target Name="Default">
    ///     <ItemGroup>
    ///       <MachineConfigSettings Include="settingName" >
    ///         <Value>settingValue</Value>
    ///       </MachineConfigSettings>
    ///     </ItemGroup>
    ///     <!-- Update machine.config app settings -->
    ///     <MSBuild.ExtensionPack.Framework.ConfigManager TaskAction="SetAppSetting" SettingName="%(MachineConfigSettings.Identity)" SettingValue="%(Value)" SaveMode="Full"/>
    ///     <ItemGroup>
    ///       <ConnectionStrings Include="myAppDB">
    ///         <Value>Server=MyServer;</Value>
    ///       </ConnectionStrings>
    ///     </ItemGroup>
    ///     <!-- Update a website's connection strings -->
    ///     <MSBuild.ExtensionPack.Framework.ConfigManager TaskAction="SetConnectionString" SettingName="%(ConnectionStrings.Identity)" SettingValue="%(Value)" ConfigurationFileType="WebConfig" Site="NewSite" Path="/" />
    ///     <MSBuild.ExtensionPack.Framework.ConfigManager TaskAction="RemoveConnectionString" SettingName="%(ConnectionStrings.Identity)" ConfigurationFileType="WebConfig"  Site="NewSite" Path="/" />
    ///     <!--- Remove a setting from a website -->
    ///     <MSBuild.ExtensionPack.Framework.ConfigManager TaskAction="RemoveAppSetting" SettingName="removeMe" ConfigurationFileType="WebConfig"  Site="NewSite" Path="/" />
    ///     <!-- Remove connection string 'obsoleteConnection' from machine.config file -->
    ///     <MSBuild.ExtensionPack.Framework.ConfigManager TaskAction="RemoveConnectionString" SettingName="obsoleteConnection" />
    ///   </Target>
    /// </Project>
    /// ]]></code>
    /// </example>
    [HelpUrl("")]
    public sealed class ConfigManager : BaseTask
    {
        internal const string RemoveAppSettingTaskAction = "RemoveAppSetting";
        internal const string RemoveConnectionStringTaskAction = "RemoveConnectionString";
        internal const string SetAppSettingTaskAction = "SetAppSetting";
        internal const string SetConnectionStringTaskAction = "SetConnectionString";
        private DotNetConfigurationFile configurationFileType = DotNetConfigurationFile.MachineConfig;
        private ConfigurationSaveMode saveMode = ConfigurationSaveMode.Minimal;
        
        /// <summary>
        /// Which .NET framework configuration file to update. Supports WebConfig and MachineConfig. Default is MachineConfig
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        [TaskAction(RemoveAppSettingTaskAction, false)]
        [TaskAction(RemoveConnectionStringTaskAction, false)]
        public string ConfigurationFileType
        {
            get { return this.configurationFileType.ToString(); }
            set { this.configurationFileType = (DotNetConfigurationFile)Enum.Parse(typeof(DotNetConfigurationFile), value); }
        }

        /// <summary>
        /// Sets the Site to work on. Leave blank to target the .net framework web.config
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        [TaskAction(RemoveAppSettingTaskAction, false)]
        [TaskAction(RemoveConnectionStringTaskAction, false)]
        public string Site { get; set; }

        /// <summary>
        /// Sets the Path to work on. Leave blank to target the .net framework web.config
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        [TaskAction(RemoveAppSettingTaskAction, false)]
        [TaskAction(RemoveConnectionStringTaskAction, false)]
        public string Path { get; set; }

        /// <summary>
        /// How should changes to the config file be saved? See 
        /// http://msdn.microsoft.com/en-us/library/system.configuration.configurationsavemode.aspx for the list of values. Default is Minimal
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        [TaskAction(RemoveAppSettingTaskAction, false)]
        [TaskAction(RemoveConnectionStringTaskAction, false)]
        public string SaveMode
        {
            get { return this.saveMode.ToString(); }
            set { this.saveMode = (ConfigurationSaveMode)Enum.Parse(typeof(ConfigurationSaveMode), value); }
        }

        /// <summary>
        /// The setting name to update.
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        [TaskAction(RemoveAppSettingTaskAction, false)]
        [TaskAction(RemoveConnectionStringTaskAction, false)]
        [Required]
        public string SettingName { get; set; }

        /// <summary>
        /// The setting's value.
        /// </summary>
        [TaskAction(SetAppSettingTaskAction, false)]
        [TaskAction(SetConnectionStringTaskAction, false)]
        public string SettingValue { get; set; }

        /// <summary>
        /// The action this task should take.  Accepted values are SetAppSetting and SetConnectionString.
        /// </summary>
        [DropdownValue(SetAppSettingTaskAction)]
        [DropdownValue(SetConnectionStringTaskAction)]
        public override string TaskAction
        {
            get { return base.TaskAction; }
            set { base.TaskAction = value; }
        }

        private Configuration Config { get; set; }

        private KeyValueConfigurationCollection AppSettings
        {
            get { return this.Config.AppSettings.Settings; }
        }

        private ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return this.Config.ConnectionStrings.ConnectionStrings; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ConfigurationFile")]
        protected override void InternalExecute()
        {
            switch (this.configurationFileType)
            {
                case DotNetConfigurationFile.MachineConfig:
                    this.Config = WebConfigurationManager.OpenMachineConfiguration();
                    break;
                case DotNetConfigurationFile.WebConfig:
                    this.Config = WebConfigurationManager.OpenWebConfiguration(this.Path, this.Site);
                    break;
                default:
                    this.Log.LogError("Task parameter ConfigurationFile has an unrecognized value.");
                    return;
            }

            switch (this.TaskAction)
            {
                case RemoveAppSettingTaskAction:
                    this.RemoveAppSetting(true);
                    break;
                case RemoveConnectionStringTaskAction:
                    this.RemoveConnectionString(true);
                    break;
                case SetAppSettingTaskAction:
                    this.SetAppSetting();
                    break;
                case SetConnectionStringTaskAction:
                    this.SetConnectionString();
                    break;
                default:
                    this.Log.LogError("Invalid task action: {0}.", this.TaskAction);
                    break;
            }
        }

        private void RemoveAppSetting(bool save)
        {
            if (this.AppSettings[this.SettingName] == null)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.InvariantCulture, "Setting not found '{0}' in {1}.", this.SettingName, this.Config.FilePath));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Removing app setting '{0}' from {1}.", this.SettingName, this.Config.FilePath));
            this.AppSettings.Remove(this.SettingName);
            if (save)
            {
                this.Save();
            }
        }

        private void SetAppSetting()
        {
            this.RemoveAppSetting(false);
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Setting app setting '{0}' in {1}.", this.SettingName, this.Config.FilePath));
            this.AppSettings.Add(this.SettingName, this.SettingValue);
            this.Save();
        }

        private void SetConnectionString()
        {
            this.RemoveConnectionString(false);
            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Setting connection string '{0}' in {1}.", this.SettingName, this.Config.FilePath));
            this.ConnectionStrings.Add(new ConnectionStringSettings(this.SettingName, this.SettingValue));
            this.Save();
        }

        private void RemoveConnectionString(bool save)
        {
            if (this.ConnectionStrings[this.SettingName] == null)
            {
                this.LogTaskMessage(MessageImportance.Low, string.Format(CultureInfo.InvariantCulture, "Setting not found '{0}' in {1}.", this.SettingName, this.Config.FilePath));
                return;
            }

            this.LogTaskMessage(string.Format(CultureInfo.InvariantCulture, "Removing connection string '{0}' from {1}.", this.SettingName, this.Config.FilePath));
            this.ConnectionStrings.Remove(this.SettingName);
            if (save)
            {
                this.Save();
            }
        }

        private void Save()
        {
            this.Config.Save(this.saveMode);
        }
    }
}