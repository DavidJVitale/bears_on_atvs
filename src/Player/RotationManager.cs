using Godot;
using System;

public enum RotationManagerState{ROTATE_SLOW_FORWARD, ROTATE_FAST_FORWARD,
                                 ROTATE_SLOW_BACKWARD, ROTATE_FAST_BACKWARD,
                                 NOT_ROTATING};

public class RotationManager : FSMNode2D<RotationManagerState>{
    public override RotationManagerState InitialState {
        get {return RotationManagerState.NOT_ROTATING;}set{}}
    
    public ATV ATV;
    private float phiRotationToApply = 0f;

    private float SecondsInAirPressingRight = 0f;
    private float SecondsInAirPressingLeft = 0f;
    private const float ROTATION_ACCELL_UNIT = 0.03f;
    private const float MAX_SLOW_ROTATION_MAG = 0.05f;
    private const float MAX_FAST_ROTATION_MAG = 0.15f;
    private const float SEC_HOLDING_BUTTON_TO_FAST_ROT = 0.8f;
    private const float FRICTION_EFFECT=0.9f;
    public override void _Ready(){
        var parent = this.GetParent();
        if(parent is ATV){
            this.ATV = (ATV)parent;}}

    private Boolean hasUILeftBeenPressedInAir;
    private Boolean hasUIRightBeenPressedInAir;

    private bool freshInAir = false;

    private void UIButtonInAirBookKeeping(float delta){
        if(this.ATV.IsInAir()){
            this.freshInAir = true;
        } else {
            this.freshInAir = false;
            this.hasUILeftBeenPressedInAir = false;
            this.hasUIRightBeenPressedInAir = false;            
        }
        if(freshInAir && Input.IsActionJustPressed("ui_left")){
            this.hasUILeftBeenPressedInAir = true;
        }
        if(freshInAir && Input.IsActionJustPressed("ui_right")){
            this.hasUIRightBeenPressedInAir = true;
        }
    }

    public override void ReactStateless(float delta){
        this.ATV.RotateTwoWheels(this.phiRotationToApply, delta);
        this.UIButtonInAirBookKeeping(delta);
        if(this.ATV.IsInAir() && 
           Input.IsActionPressed("ui_left") &&
           Input.IsActionPressed("ui_right")){
               this.SecondsInAirPressingLeft = 0f;
               this.SecondsInAirPressingRight = 0f;
        }
        if(this.ATV.IsInAir() &&
           Input.IsActionPressed("ui_left")){
                this.SecondsInAirPressingLeft += delta;}
        else{
                this.SecondsInAirPressingLeft = 0f;}
        if(this.ATV.IsInAir() &&
           Input.IsActionPressed("ui_right")){
                this.SecondsInAirPressingRight += delta;
        } else {
                this.SecondsInAirPressingRight = 0f;}}

    private void playRotateSound(){
                SoundHandler.PlaySample<MyAudioStreamPlayer2D>(this.ATV.Bear,
                    new string[] {"res://media/samples/player/flip_1.wav"},
                    Loop: true,
                    SkipIfAlreadyPlaying: true);
    }

    private void stopRotateSound(){
                SoundHandler.StopSample(this.ATV.Bear,
                    "res://media/samples/player/flip_1.wav");}

    public override void ReactToState(float delta){
        switch(this.ActiveState){
            case RotationManagerState.ROTATE_SLOW_FORWARD:
                this.stopRotateSound();
                this.ATV.CancelAllRotationalEnergy();
                this.ATV.CancelAllBackwardTwoWheelEnergy();
                if(Math.Abs(this.phiRotationToApply) < MAX_SLOW_ROTATION_MAG){
                    this.phiRotationToApply += ROTATION_ACCELL_UNIT;
                } else if (this.phiRotationToApply < 0f){
                    this.phiRotationToApply = 0f;}
                break;
            case RotationManagerState.ROTATE_FAST_FORWARD:
                this.playRotateSound();
                this.ATV.CancelAllRotationalEnergy();
                this.ATV.CancelAllBackwardTwoWheelEnergy();
                if(Math.Abs(this.phiRotationToApply) < MAX_FAST_ROTATION_MAG){
                    this.phiRotationToApply += ROTATION_ACCELL_UNIT;
                } else if (this.phiRotationToApply < 0f){
                    this.phiRotationToApply = 0f;}
                break;
            case RotationManagerState.ROTATE_SLOW_BACKWARD:
                this.stopRotateSound();
                this.ATV.CancelAllRotationalEnergy();
                this.ATV.CancelAllForwardTwoWheelEnergy();
                if(Math.Abs(this.phiRotationToApply) < MAX_SLOW_ROTATION_MAG){
                    this.phiRotationToApply -= ROTATION_ACCELL_UNIT;
                } else if (this.phiRotationToApply > 0f ){
                    this.phiRotationToApply = 0f;}
                break;
            case RotationManagerState.ROTATE_FAST_BACKWARD:
                this.playRotateSound();
                this.ATV.CancelAllRotationalEnergy();
                this.ATV.CancelAllForwardTwoWheelEnergy();
                if(Math.Abs(this.phiRotationToApply) < MAX_FAST_ROTATION_MAG){
                    this.phiRotationToApply -= ROTATION_ACCELL_UNIT;
                } else if (this.phiRotationToApply > 0f ){
                    this.phiRotationToApply = 0f;}
                break;
            case RotationManagerState.NOT_ROTATING:
                this.stopRotateSound();
                this.phiRotationToApply = 0f;
                break;}}

    public override void UpdateState(float delta){
        if(this.ATV.IsInAirNormalized() && this.ATV.ActiveState == ATVState.WITH_BEAR){
            ///If we're in the right situation to be able to accept input
            if(Input.IsActionPressed("ui_right") && Input.IsActionPressed("ui_left")){
                this.SetActiveState(RotationManagerState.NOT_ROTATING, 100);}
            else if(Input.IsActionPressed("ui_right") && this.hasUIRightBeenPressedInAir){
                if(this.SecondsInAirPressingRight > SEC_HOLDING_BUTTON_TO_FAST_ROT){
                    this.SetActiveState(RotationManagerState.ROTATE_FAST_FORWARD, 100);}
                else if(this.SecondsInAirPressingRight <= SEC_HOLDING_BUTTON_TO_FAST_ROT){
                   this.SetActiveState(RotationManagerState.ROTATE_SLOW_FORWARD, 100);}
                else{
                    this.SetActiveState(RotationManagerState.NOT_ROTATING, 100);}}
            else if(Input.IsActionPressed("ui_left") && this.hasUILeftBeenPressedInAir){
               if(this.SecondsInAirPressingLeft > SEC_HOLDING_BUTTON_TO_FAST_ROT){
                    this.SetActiveState(RotationManagerState.ROTATE_FAST_BACKWARD, 100);}
               else if (this.SecondsInAirPressingLeft <= SEC_HOLDING_BUTTON_TO_FAST_ROT){
                   this.SetActiveState(RotationManagerState.ROTATE_SLOW_BACKWARD, 100);}
               else{
                    this.SetActiveState(RotationManagerState.NOT_ROTATING, 100);}}
            else{
                this.SetActiveState(RotationManagerState.NOT_ROTATING, 100);}
        } else {
            this.SetActiveState(RotationManagerState.NOT_ROTATING, 100);}}
    }