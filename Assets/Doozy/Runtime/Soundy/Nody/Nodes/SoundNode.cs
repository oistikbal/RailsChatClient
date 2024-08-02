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
    /// Soundy node that allows you to perform actions on all sounds, on a sound library or on a sound.
    /// This node is used by Nody, a visual navigation graph tool available inside Doozy UI Manager (https://doozyui.com/features-nody/)
    /// </summary>
    [Serializable]
    [NodyMenuPath("Soundy", "Sound")]
    public sealed class SoundNode : SimpleNode
    {
        /// <summary> Action to perform on all sounds </summary>
        public SoundActionType AllSoundsAction = SoundActionType.DoNothing;

        /// <summary> Sound Library Id </summary>
        public SoundLibraryId SoundLibraryId = new SoundLibraryId();

        /// <summary> Action to perform on the sound library identified by the SoundLibraryId </summary>
        public LibraryActionType LibraryAction = LibraryActionType.DoNothing;

        /// <summary> Sound Id </summary>
        public SoundId SoundId = new SoundId();

        /// <summary> Action to perform on the sound identified by the SoundId </summary>
        public AudioActionType SoundAction = AudioActionType.Play;

        public SoundNode()
        {
            AddInputPort()                 // add a new input port
                .SetCanBeDeleted(false)    // port options
                .SetCanBeReordered(false); // port options

            AddOutputPort()                // add a new output port
                .SetCanBeDeleted(false)    // port options
                .SetCanBeReordered(false); // port options

            canBeDeleted = true; // Used to prevent special nodes from being deleted in the editor

            runUpdate = false;      // Run Update when the node is active
            runFixedUpdate = false; // Run FixedUpdate when the node is active
            runLateUpdate = false;  // Run LateUpdate when the node is active

            passthrough = true; //allow the graph to bypass this node when going back

            clearGraphHistory = false; //remove the possibility of being able to go back to previously active nodes
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
            ExecuteAllSoundsAction();
            ExecuteLibraryAction();
            ExecuteSoundAction();
        }

        public void ExecuteAllSoundsAction()
        {
            switch (AllSoundsAction)
            {
                case SoundActionType.DoNothing: 
                    //do nothing
                    break;
                
                case SoundActionType.StopAllSounds:
                    SoundyService.StopAllSounds();
                    break;
                
                case SoundActionType.FadeOutAndStopAllSounds:
                    SoundyService.FadeOutAndStopAllSounds();
                    break;
                
                case SoundActionType.PauseAllSounds:
                    SoundyService.PauseAllSounds();
                    break;
                
                case SoundActionType.UnPauseAllSounds:
                    SoundyService.UnPauseAllSounds();
                    break;
                
                case SoundActionType.MuteAllSounds:
                    SoundyService.MuteAllSounds();
                    break;
                
                case SoundActionType.UnMuteAllSounds:
                    SoundyService.UnMuteAllSounds();
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
                    SoundyService.StopSoundLibrary(SoundLibraryId);
                    break;
                
                case LibraryActionType.FadeOutAndStopLibrary:
                    SoundyService.FadeOutAndStopSoundLibrary(SoundLibraryId);
                    break;
                
                case LibraryActionType.PauseLibrary:
                    SoundyService.PauseSoundLibrary(SoundLibraryId.libraryName);
                    break;
                
                case LibraryActionType.UnPauseLibrary:
                    SoundyService.UnPauseSoundLibrary(SoundLibraryId);
                    break;
                
                case LibraryActionType.MuteLibrary:
                    SoundyService.MuteSoundLibrary(SoundLibraryId);
                    break;
                
                case LibraryActionType.UnMuteLibrary:
                    SoundyService.UnMuteSoundLibrary(SoundLibraryId);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ExecuteSoundAction()
        {
            switch (SoundAction)
            {
                case AudioActionType.DoNothing:
                    //do nothing
                    break;
                
                case AudioActionType.Play:
                    SoundyService.PlaySound(SoundId);    
                    break;
                
                case AudioActionType.Stop:
                    SoundyService.StopSound(SoundId);
                    break;
               
                case AudioActionType.FadeOutAndStop:
                    SoundyService.FadeOutAndStopSound(SoundId);
                    break;
                
                case AudioActionType.Pause:
                    SoundyService.PauseSound(SoundId);
                    break;
                
                case AudioActionType.UnPause:
                    SoundyService.UnPauseSound(SoundId);
                    break;
                
                case AudioActionType.Mute:
                    SoundyService.MuteSound(SoundId);
                    break;
                
                case AudioActionType.UnMute:
                    SoundyService.UnMuteSound(SoundId);
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
