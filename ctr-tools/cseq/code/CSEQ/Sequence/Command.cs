﻿using System;
using System.IO;
using NAudio.Midi;
using CTRtools.Helpers;
using System.Collections.Generic;

namespace CTRtools.CSEQ
{
    public enum CSEQEvent
    {
        NoteOff = 0x01,
        EndTrack2 = 0x02,
        EndTrack = 0x03,
        Unknown4 = 0x04,
        NoteOn = 0x05,
        VelAssume = 0x06,
        PanAssume = 0x07,
        Unknown8 = 0x08,
        ChangePatch = 0x09,
        BendAssume = 0x0A,
        Error = 0xFF
    }


    public class Command
    {
        public CSEQEvent evt;
        public byte pitch;
        public byte velocity;
        public int wait;

        public void Read(BinaryReaderEx br)
        {
            wait = br.ReadTimeDelta();

            byte op = br.ReadByte();

            evt = (CSEQEvent)op;

            switch (evt)
            {
                case CSEQEvent.Unknown4:
                case CSEQEvent.Unknown8:
                    {
                        pitch = br.ReadByte();
                        Log.Write(op.ToString("X2") + " found at " + br.HexPos() + "\r\n");
                        break;
                    }

                case CSEQEvent.EndTrack2:
                case CSEQEvent.ChangePatch:
                case CSEQEvent.BendAssume:
                case CSEQEvent.VelAssume:
                case CSEQEvent.PanAssume:
                case CSEQEvent.NoteOff:
                    {
                        pitch = br.ReadByte();
                        break;
                    }
                case CSEQEvent.NoteOn:
                    {
                        pitch = br.ReadByte();
                        velocity = br.ReadByte();
                        break;
                    }
                case CSEQEvent.EndTrack:
                    {
                        break;
                    }

                default:
                    {
                        evt = CSEQEvent.Error;
                        Log.Write(op.ToString("X2") + " not recognized at " + br.HexPos() + "\r\n");
                        break;
                    }
            }
        }

        public List<MidiEvent> ToMidiEvent(int absTime, int channel, CSEQ seq, CTrack ct)
        {
            List<MidiEvent> events = new List<MidiEvent>();
            //TrackPatch tp = new TrackPatch();

            absTime += wait;

            //we can't go beyond 16 with midi
            channel = (channel <= 16) ? channel : 16;

            if (CSEQ.PatchMidi)
            {
                if (ct.isDrumTrack)
                {
                    if (evt == CSEQEvent.NoteOn || evt == CSEQEvent.NoteOff)
                    {
                        pitch = (byte)seq.shortSamples[pitch].info.Key;
                    }
                }
                    
                else
                {
                    if (evt == CSEQEvent.ChangePatch)
                    {
                        CSEQ.ActiveInstrument = pitch;
                        pitch = (byte)seq.longSamples[CSEQ.ActiveInstrument].info.Midi;
                    }
                    else if (evt == CSEQEvent.NoteOn || evt == CSEQEvent.NoteOff)
                    {
                        try
                        {
                            pitch += (byte)seq.longSamples[CSEQ.ActiveInstrument].info.Pitch;
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                }

            }


            switch (evt)
            {
                case CSEQEvent.NoteOn: events.Add(new NoteEvent(absTime, channel, MidiCommandCode.NoteOn, pitch, velocity)); break;
                case CSEQEvent.NoteOff: events.Add( new NoteEvent(absTime, channel, MidiCommandCode.NoteOff, pitch, velocity)); break;

                case CSEQEvent.ChangePatch:
                   // events.Add(new ControlChangeEvent(absTime, channel, MidiController.MainVolume, seq.longSamples[pitch].velocity / 2));
                    events.Add(new PatchChangeEvent(absTime, channel, pitch)); 
                    break;

                case CSEQEvent.BendAssume: events.Add( new PitchWheelChangeEvent(absTime, channel, pitch * 64)); break;
                case CSEQEvent.PanAssume: events.Add( new ControlChangeEvent(absTime, channel, MidiController.Pan, pitch / 2)); break;
                case CSEQEvent.VelAssume: events.Add( new ControlChangeEvent(absTime, channel, MidiController.MainVolume, pitch / 2)); break; //not really used

                case CSEQEvent.EndTrack2:
                case CSEQEvent.EndTrack: events.Add(new MetaEvent(MetaEventType.EndTrack, 0, absTime)); break;
            }

            return events;
        }

        public override string ToString()
        {
            return String.Format("{0}t - {1}[p:{2}, v:{3}]\r\n", wait, evt.ToString(), pitch, velocity);
        }


        public void WriteBytes(BinaryWriter bw)
        {

            int value = wait;

            int buffer = value & 0x7F;

            while (value != (value >> 7))
            {
                value = value >> 7;
                buffer <<= 8;
                buffer |= ((value & 0x7F) | 0x80);
            }

            while (true)
            {
                bw.Write((byte)buffer);
                if ((buffer & 0x80) > 0)
                {
                    buffer >>= 8;
                }
                else
                {
                    break;
                }
            }


            bw.Write((byte)evt);

            switch (evt)
            {
                case CSEQEvent.Unknown4:
                case CSEQEvent.Unknown8:
                case CSEQEvent.EndTrack2:
                case CSEQEvent.ChangePatch:
                case CSEQEvent.BendAssume:
                case CSEQEvent.VelAssume:
                case CSEQEvent.PanAssume:
                case CSEQEvent.NoteOff:
                    {
                        bw.Write((byte)pitch);
                        break;
                    }

                case CSEQEvent.NoteOn:
                    {
                        bw.Write((byte)pitch);
                        bw.Write((byte)velocity);
                        break;
                    }

                case CSEQEvent.EndTrack:
                    {
                        break;
                    }
            }
        }
    }
}