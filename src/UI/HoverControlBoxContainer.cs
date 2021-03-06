using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HoverControlBoxContainer : VBoxContainer {
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public List<Node> HoverableItems = new List<Node>();
    public int HoverableItemIndex = 0;
    public Node HoveredItem { get { return this.HoverableItems[this.HoverableItemIndex];}
                              set { var index = this.HoverableItems.FindIndex( x => x.Equals(value));
                                    this.HoverableItemIndex = index;
                                    this.updateHovered(); }}
    public override void _Ready(){
        this.updateHovered();
        foreach(Node child in this.GetChildren()){
            if(child.GetChildCount() != 0 && child.GetChild(0) is TouchScreenButton){
                HoverableItems.Add((Node)child.GetChild(0));}
            if(child.GetChildCount() != 0 && child.GetChild(0) is SlotArea){
                HoverableItems.Add((Node)child.GetChild(0));}
            else if(child is OptionButton){
                var button = (OptionButton)child;
                button.GetPopup().FocusMode = FocusModeEnum.All;
                HoverableItems.Add(button);}}}

    private void IncrementMenuDown(){
        if(this.HoverableItemIndex < this.HoverableItems.Count-1){
            this.HoverableItemIndex++;}}

    private void IncrementOptionButtonDown(OptionButton button){
        var i = button.Selected;
        if(i < button.Items.Count -1){
            i++;
            button.Select(i);}}

    private void IncrementMenuUp(){
        if(this.HoverableItemIndex > 0){
            this.HoverableItemIndex--;}}

    private void IncrementOptionButtonUp(OptionButton button){
        var i = button.Selected;
        if(i > 0){
            i--;
            button.Select(i);}}

    private void updateHovered(){
        foreach(Node item in this.HoverableItems){
            if(item is HoverableTouchScreenButton){
                var button = (HoverableTouchScreenButton)item;
                if(button.Equals(this.HoveredItem)){
                    button.SetGraphicToPressed();
                } else {
                    button.SetGraphicToUnpressed();}}
            else if(item is OptionButton){
                var button = (OptionButton)item;
                button.SetFocusMode(FocusModeEnum.All);
                if(button.Equals(this.HoveredItem)){
                    button.GrabFocus();}
                else{
                    button.ReleaseFocus();}}
            else if(item is SlotArea){
                var slotArea = (SlotArea)item;
                if(slotArea.Equals(this.HoveredItem)){
                    slotArea.SetToHovered();}
                else{
                    slotArea.SetToUnhovered();}}
            
            }}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta){
        this.updateHovered();
        if(this.Visible == false){
            this.HoverableItemIndex = 0;
        } else {
            if(Input.IsActionJustPressed("ui_down")){
                    SoundHandler.PlaySample<MyAudioStreamPlayer>(this,
                        new string[]{"res://media/samples/ui/click_1.wav"});
                if(this.HoveredItem is OptionButton && ((OptionButton)this.HoveredItem).GetPopup().IsVisible()){
                    var button = (OptionButton)this.HoveredItem;
                    this.IncrementOptionButtonDown(button);
                } else {
                    this.IncrementMenuDown();}}
            if(Input.IsActionJustPressed("ui_up")){
                    SoundHandler.PlaySample<MyAudioStreamPlayer>(this,
                        new string[]{"res://media/samples/ui/click_1.wav"});
                if(this.HoveredItem is OptionButton && ((OptionButton)this.HoveredItem).GetPopup().IsVisible()){
                    var button = (OptionButton)this.HoveredItem;
                    this.IncrementOptionButtonUp(button);}     
                else {
                this.IncrementMenuUp();}}
            if(Input.IsActionJustPressed("ui_accept")){
                if(this.HoveredItem is HoverableTouchScreenButton){
                    ((HoverableTouchScreenButton)this.HoveredItem).MimicTouch();}
                if(this.HoveredItem is OptionButton){
                    var button = (OptionButton)this.HoveredItem;
                     if(!this.isOptionButtonPopupOpened(button)){
                        this.closeOptionButtonWorkaround(button);}}
                if(this.HoveredItem is SlotArea){
                    ((SlotArea)this.HoveredItem).MimicTouch();}}
            if(Input.IsActionJustReleased("ui_accept")){
                if(this.HoveredItem is HoverableTouchScreenButton){
                    ((HoverableTouchScreenButton)this.HoveredItem).MimicTouchRelease();}
                if(this.HoveredItem is SlotArea){
                    ((SlotArea)this.HoveredItem).MimicTouchRelease();}}}}

    private Boolean isOptionButtonPopupOpened(OptionButton button){
        return (button.GetPopup().Visible == true) && //Butto `Pressed` status gets updated
                (button.Pressed); //1 frame after, so it's value here is reveresed
    }

  private void closeOptionButtonWorkaround(OptionButton button){
      //TODO: find better solution
        button.GetPopup().Visible = false;
        this.IncrementMenuDown();
        this.updateHovered();
        this.IncrementMenuUp();
        this.updateHovered();}}