// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if DOOZY_UIMANAGER

using System;
using Doozy.Runtime.Nody;
using Doozy.Runtime.Nody.Nodes.Internal;
using Doozy.Runtime.Soundy.Ids;
// ReSharper disable RedundantOverriddenMember

namespace Doozy.Runtime.Soundy.Nody.Nodes
{
    /// <summary>
    /// Soundy node that allows you to perform actions on all music, a music library or a music.
    /// This node is used by Nody, a visual navigation graph tool available inside Doozy UI Manager (https://doozyui.com/features-nody/)
    /// </summary>
    [Serializable]
    [NodyMenuPath("Soundy", "Music")]
    public sealed class MusicNode : SimpleNode
    {
        /// <summary> Action to perform on all music </summary>
        public MusicActionType AllMusicAction = MusicActionType.DoNothing;
        
        /// <summary> Music Library Id </summary>
        public MusicLibraryId MusicLibraryId = new MusicLibraryId();
        
        /// <summary> Action to perform on the music library identified by the MusicLibraryId </summary>
        public LibraryActionType LibraryAction = LibraryActionType.DoNothing;
        
        /// <summary> Music Id </summary>
        public MusicId MusicId = new MusicId();
        
        /// <summary> Action to perform on the music identified by the MusicId </summary>
        public AudioActionType MusicAction = AudioActionType.Play;
        
        public MusicNode()
        {
            AddInputPort()                 // add a new input port
                .SetCanBeDeleted(false)    // port options
                .SetCanBeReordered(false); // port options

            AddOutputPort()                // add a new output port
                .SetCanBeDeleted(false)    // port options
                .SetCanBeReordered(false); // port options

            canBeDeleted = true;           // Used to prevent special nodes from being deleted in the editor
            
            runUpdate = false;             // Run Update when the node is active
            runFixedUpdate = false;        // Run FixedUpdate when the node is active
            runLateUpdate = false;         // Run LateUpdate when the node is active
            
            passthrough = true;            //allow the graph to bypass this node when going back
                
            clearGraphHistory = false;     //remove the possibility of being able to go back to previously active nodes
        }

        // Called on the frame when this node becomes active
        public override void OnEnter(FlowNode previousNode = null, FlowPort previousPort = null)
        {
            base.OnEnter(previousNode, previousPort);
            Run();                         //do something
            GoToNextNode(firstOutputPort); //immediately go to the next node
        }
        
        private void Run()
        {
            ExecuteAllMusicAction();
            ExecuteLibraryAction();
            ExecuteMusicAction();
        }

        public void ExecuteAllMusicAction()
        {
            switch (AllMusicAction)
            {
                case MusicActionType.DoNothing:
                    //do nothing
                    break;
                
                case MusicActionType.StopAllMusic:
                    SoundyService.StopAllMusic();
                    break;
                
                case MusicActionType.FadeOutAndStopAllMusic:
                    SoundyService.FadeOutAndStopAllMusic();
                    break;
                
                case MusicActionType.PauseAllMusic:
                    SoundyService.PauseAllMusic();
                    break;
                
                case MusicActionType.UnPauseAllMusic:
                    SoundyService.UnPauseAllMusic();
                    break;
                
                case MusicActionType.MuteAllMusic:
                    SoundyService.MuteAllMusic();
                    break;
                
                case MusicActionType.UnMuteAllMusic:
                    SoundyService.UnMuteAllMusic();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }  
        }

        public void ExecuteLibraryAction()
        {
            switch (LibraryAction)
            {
                case LibraryActionType.DoNothing:
                    //do nothing
                    break;
                
                case LibraryActionType.StopLibrary:
                    SoundyService.StopMusicLibrary(MusicLibraryId);
                    break;
                
                case LibraryActionType.FadeOutAndStopLibrary:
                    SoundyService.FadeOutAndStopMusicLibrary(MusicLibraryId);
                    break;
                
                case LibraryActionType.PauseLibrary:
                    SoundyService.PauseMusicLibrary(MusicLibraryId);
                    break;
                
                case LibraryActionType.UnPauseLibrary:
                    SoundyService.UnPauseMusicLibrary(MusicLibraryId);
                    break;
                
                case LibraryActionType.MuteLibrary:
                    SoundyService.MuteMusicLibrary(MusicLibraryId);
                    break;
                
                case LibraryActionType.UnMuteLibrary:
                    SoundyService.UnMuteMusicLibrary(MusicLibraryId);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ExecuteMusicAction()
        {
            switch (MusicAction)
            {
                case AudioActionType.DoNothing:
                    //do nothing
                    break;
                
                case AudioActionType.Play:
                    SoundyService.PlayMusic(MusicId);
                    break;
                
                case AudioActionType.Stop:
                    SoundyService.StopMusic(MusicId);
                    break;
                
                case AudioActionType.FadeOutAndStop:
                    SoundyService.FadeOutAndStopMusic(MusicId);
                    break;
                
                case AudioActionType.Pause:
                    SoundyService.PauseMusic(MusicId);
                    break;
                
                case AudioActionType.UnPause:
                    SoundyService.UnPauseMusic(MusicId);
                    break;
                
                case AudioActionType.Mute:
                    SoundyService.MuteMusic(MusicId);
                    break;
                
                case AudioActionType.UnMute:
                    SoundyService.UnMuteMusic(MusicId);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Clone this node
        public override FlowNode Clone()
        {
            return base.Clone();
            //clone custom settings
        }

        // Override - Add a new port to this node
        public override FlowPort AddPort(PortDirection direction, PortCapacity capacity)
        {
            FlowPort port = base.AddPort(direction, capacity);
            //add port value
            return port;
        }

        // Override - Add a new input port to this node
        public override FlowPort AddInputPort(PortCapacity capacity = PortCapacity.Multi)
        {
            FlowPort port = base.AddInputPort(capacity);
            //add input port value
            return port;
        }

        // Override - Add a new output port to this node
        public override FlowPort AddOutputPort(PortCapacity capacity = PortCapacity.Single)
        {
            FlowPort port = base.AddOutputPort(capacity);
            //add output port value
            return port;
        }
    }
}

#endif
