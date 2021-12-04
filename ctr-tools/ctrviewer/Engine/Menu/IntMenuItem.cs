﻿using System;

namespace ctrviewer.Engine.Gui
{ 
    public class IntMenuItem : MenuItem
    {
        private int _value = 0;

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public IntMenuItem(int InitValue)
        {
            _value = InitValue;
        }
    }
}