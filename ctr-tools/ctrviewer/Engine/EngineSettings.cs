﻿using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Xml;

namespace ctrviewer
{
    class EngineSettings
    {
        public static string SettingsFile = "settings.xml";

        public Vector2 Resolution = Vector2.Zero;

        public byte AntiAliasLevel { get; set; } = 4;
        public bool TextureFiltering { get; set; } = true;
        public bool VisData { get; set; } = false;
        public bool GenerateMips { get; set; } = true;
        public bool Sky { get; set; } = true;
        public bool BotsPath { get; set; } = false;
        public bool Models { get; set; } = false;
        public bool StereoPair { get; set; } = false;
        public int StereoPairSeparation { get; set; } = 20;
        public bool ShowCamPos { get; set; } = false;
        public bool UseLowLod { get; set; } = false;

        private bool _windowed = true;
        public bool Windowed
        {
            get
            {
                return _windowed;
            }
            set
            {
                _windowed = value;
                if (onWindowedChanged != null)
                    onWindowedChanged();
            }
        }

        private bool _vertexLighting = true;
        public bool VertexLighting
        {
            get
            {
                return _vertexLighting;
            }
            set
            {
                _vertexLighting = value;
                if (onVertexLightingChanged != null)
                    onVertexLightingChanged();
            }
        }

        private bool _antiAlias = true;
        public bool AntiAlias
        {
            get
            {
                return _antiAlias;
            }
            set
            {
                _antiAlias = value;
                if (onAntiAliasChanged != null)
                    onAntiAliasChanged();
            }
        }

        private bool _verticalSync = true;
        public bool VerticalSync
        {
            get
            {
                return _verticalSync;
            }
            set
            {
                _verticalSync = value;
                if (onVerticalSyncChanged != null)
                    onVerticalSyncChanged();
            }
        }

        private int _fieldOfView = 80;
        public int FieldOfView
        {
            get
            {
                if (_fieldOfView < 20) _fieldOfView = 20;
                if (_fieldOfView > 150) _fieldOfView = 150;
                return _fieldOfView;
            }
            set
            {
                _fieldOfView = value;
                if (onFieldOfViewChanged != null)
                    onFieldOfViewChanged();
            }
        }

        private int _windowScale = 75;
        public int WindowScale
        {
            get
            {
                if (_windowScale < 10) _fieldOfView = 10;
                if (_windowScale > 90) _fieldOfView = 90;
                return _fieldOfView;
            }
            set
            {
                _fieldOfView = value;
                if (onWindowedChanged != null)
                    onWindowedChanged();
            }
        }

        public delegate void DelegateNoArgs();

        public DelegateNoArgs onWindowedChanged = null;
        public DelegateNoArgs onVertexLightingChanged = null;
        public DelegateNoArgs onAntiAliasChanged = null;
        public DelegateNoArgs onVerticalSyncChanged = null;
        public DelegateNoArgs onFieldOfViewChanged = null;

        public EngineSettings()
        {
        }

        public static EngineSettings Load()
        {
            if (!File.Exists(SettingsFile))
                return new EngineSettings();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText(SettingsFile));

            XmlNode vid = xml.SelectNodes("/settings")[0];

            //fix, this fails if node is missing
            return new EngineSettings()
            {
                AntiAlias = Boolean.Parse(vid["AntiAlias"].InnerText),
                AntiAliasLevel = Byte.Parse(vid["AntiAliasLevel"].InnerText),
                Windowed = Boolean.Parse(vid["Windowed"].InnerText),
                TextureFiltering = Boolean.Parse(vid["TextureFiltering"].InnerText),
                VerticalSync = Boolean.Parse(vid["VerticalSync"].InnerText),
                VertexLighting = Boolean.Parse(vid["VertexLighting"].InnerText),
                VisData = Boolean.Parse(vid["VisData"].InnerText),
                GenerateMips = Boolean.Parse(vid["GenerateMips"].InnerText),
                Sky = Boolean.Parse(vid["Sky"].InnerText),
                Models = Boolean.Parse(vid["Models"].InnerText),
                BotsPath = Boolean.Parse(vid["BotsPath"].InnerText),
                StereoPair = Boolean.Parse(vid["StereoPair"].InnerText),
                StereoPairSeparation = Int32.Parse(vid["StereoPairSeparation"].InnerText),
                FieldOfView = Int32.Parse(vid["FieldOfView"].InnerText),
                ShowCamPos = Boolean.Parse(vid["ShowCamPos"].InnerText),
                WindowScale = Int32.Parse(vid["WindowScale"].InnerText),
                UseLowLod = Boolean.Parse(vid["UseLowLod"].InnerText)
            };
        }
        public static void Save(string path)
        {
            throw new NotImplementedException();
        }
    }
}
