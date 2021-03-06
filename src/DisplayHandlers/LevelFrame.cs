using Godot;
using System;

public class LevelFrame : Node{

    private PauseMenu PauseMenu;
    public Viewport Viewport;
    public Player Player;
    public ILevel Level { get {
        return this.Player.ActiveLevel;}}
    public Label TitleLabel;
    public Label ZoneLabel;
    public PlayerStatsDisplayHandlerContainer PlayerStatsDisplayHandlerContainer;
    public PlayerStatsDisplayHandler PlayerStatsDisplayHandler { get {
        return this.PlayerStatsDisplayHandlerContainer.PlayerStatsDisplayHandler;}}

    public DialogueHandler DialogueHandler;

    public BossFightLevelFrameHandler BossFightLevelFrameHandler;
    public TrackingArrowHandler TrackingArrowHandler;

    [Export]
    public NodePath UnderHudTranspPath {get; set;}
    private Sprite underHudTransp;
    public Sprite UnderHudTransp { get {
        if(this.underHudTransp is null){
            this.underHudTransp = this.GetNode<Sprite>(this.UnderHudTranspPath);}
        return this.underHudTransp;
    }}

    public override void _Ready(){
        foreach(Node child in this.GetChildren()){
            if(child is PauseMenu){
                this.PauseMenu = (PauseMenu)child;}
            if(child is ViewportContainer){
                this.Viewport = (Viewport)(child.GetChild(0));
                this.Player = this._recursGetPlayer(this.Viewport);}
            if(child is PlayerStatsDisplayHandlerContainer){
                this.PlayerStatsDisplayHandlerContainer = (PlayerStatsDisplayHandlerContainer)child;}
            if(child is DialogueHandler){
                this.DialogueHandler = (DialogueHandler)child;}
            if(child is BossFightLevelFrameHandler){
                this.BossFightLevelFrameHandler = (BossFightLevelFrameHandler)child;}
            if(child is TrackingArrowHandler){
                this.TrackingArrowHandler = (TrackingArrowHandler)child;}
            if(child.Name.ToLower().Contains("levelenter")){
                foreach(Node subchild in child.GetChildren()){
                    if(subchild.Name.ToLower().Contains("background")){
                        this.TitleLabel = (Label)subchild.GetChild(0);
                        this.ZoneLabel = (Label)subchild.GetChild(1);}}}
        }
    }

    private Player _recursGetPlayer(Node node){
        if(node is Player){
            return (Player)node;}
        else{
            foreach(Node child in node.GetChildren()){
                var potPlayer = this._recursGetPlayer(child);
                if(potPlayer != null){
                    return potPlayer;}}
            return null;}
    }

    public void SetLevelTitle(String title, int zone = -1){
        this.TitleLabel.Text = title;
        if(zone == -1){
            this.ZoneLabel.Text = "";}
        else{
            this.ZoneLabel.Text = this.Tr("ZONE") + " " + zone.ToString();}
    }

    public override void _Process(float delta){
        if(Input.IsActionJustPressed("ui_pause") && 
           this.Player.ActiveState.Equals(PlayerState.ALIVE)){
           this.PauseMenu.OpenPauseMenu(this);}    
    }


}