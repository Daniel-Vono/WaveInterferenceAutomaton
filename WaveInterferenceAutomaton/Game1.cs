// File Name: Game1.cs
// Creation Date: Apr. 29, 2024
// Description: The driver class

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GameUtility;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{

    private static byte gridWidth = 24;
    private static byte gridHeight = 23;

    private const int UI_WIDTH = 192;
    private static readonly int SCREEN_WIDTH = gridWidth * Tile.DIMENSION + UI_WIDTH;
    private static readonly int SCREEN_HEIGHT = gridHeight * Tile.DIMENSION;

    private static readonly Rectangle uiBgRect = new Rectangle(gridWidth * Tile.DIMENSION, 0, UI_WIDTH, SCREEN_HEIGHT);

    private const int LEFT_MOUSE = 0;
    private const int RIGHT_MOUSE = 1;
    private const int MIDDLE_MOUSE = 2;

    private const string INSTRUCTIONS_TEXT = "0: Reset\n1: Two Point Interference\n2: Double Slit\n\nLeft Click: Wave\nMid Click: Emitter\nRight Click: Toggle Wall";
    private readonly Vector2 instructionsTextPos = new Vector2(gridWidth * Tile.DIMENSION + 5, 5);

    public const short UPDATE_TIMER_TIME = 60;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D tileImg;

    private SpriteFont msgFont;

    public static Tile[,] Grid { get; private set; }
    public static float[,] Superpositions { get; private set; }

    public static byte GlobalUpdateId { get; private set; }

    private Timer updateTimer;

    private MouseState mouse;
    private MouseState prevMouse;

    private KeyboardState kb;
    private KeyboardState prevKb;

    private Queue<Action> pending = new Queue<Action>();
    private List<Tuple<byte, byte>> usedCoords;
    private Action dequeuedAction;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        //Updates and applies a new screen width and height 
        graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
        graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
        graphics.ApplyChanges();

        base.Initialize();

        //Makes the mouse visible
        IsMouseVisible = true;

        GlobalUpdateId = 0;

        usedCoords = new List<Tuple<byte, byte>>();
    }

    public static byte GetGridWidth()
    {
        return gridWidth;
    }

    public static byte GetGridHeight()
    {
        return gridHeight;
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        tileImg = new Texture2D(graphics.GraphicsDevice, Tile.DIMENSION, Tile.DIMENSION);
        Color[] data = new Color[Tile.DIMENSION * Tile.DIMENSION];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new Color(255, 255, 255, 255);
        }
        tileImg.SetData(data);

        msgFont = Content.Load<SpriteFont>("Fonts/MsgFont");

        Grid = new Tile[gridHeight, gridWidth];

        updateTimer = new Timer(UPDATE_TIMER_TIME, true);

        ClearGrid();
    }

    private void ClearGrid()
    {
        updateTimer.ResetTimer(true);

        Superpositions = new float[gridHeight, gridWidth];

        for (byte row = 0; row < gridHeight; row++)
        {
            for (byte column = 0; column < gridWidth; column++)
            {
                Grid[row, column] = new OpenTile(row, column);
            }
        }
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
        //Unloads the tile image because it was not created using the content manager
        if (tileImg != null)
        {
            tileImg.Dispose();
            tileImg = null;
        }
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        if (kb.IsKeyDown(Keys.Escape)) Exit();

        updateTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

        prevMouse = mouse;
        mouse = Mouse.GetState();

        prevKb = kb;
        kb = Keyboard.GetState();

        if (NewKeyPress(Keys.D0)) ClearGrid();
        else if (NewKeyPress(Keys.D1)) AddTwoPointSourceInterference();
        else if (NewKeyPress(Keys.D2)) AddDoubleSlit();

        #region Simulation Update

        if (updateTimer.IsFinished())
        {
            usedCoords.Clear();

            #region Do All Pending Actions
            
            while (pending.Count != 0)
            {
                dequeuedAction = pending.Dequeue();

                for (byte i = 0; i < usedCoords.Count; i++)
                {
                    if(dequeuedAction.Coord.Equals(usedCoords[i]))
                    {
                        continue;
                    }
                }

                switch(dequeuedAction.Type)
                {
                    case Action.ActionType.Waves:
                        Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2].AddParticle(PropagationState.Right, PropagationState.Right, Particle.STARTING_ENERGY, (byte)(GlobalUpdateId + 1));
                        Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2].AddParticle(PropagationState.Left, PropagationState.Left, Particle.STARTING_ENERGY, (byte)(GlobalUpdateId + 1));
                        break;

                    case Action.ActionType.AbsorbWall:

                        if(Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] is OpenTile)
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new AbsorbWall(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2);
                            Superpositions[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = AbsorbWall.SUPERPOSITION;
                        }
                        else
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new OpenTile(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2);
                            Superpositions[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = 0f;
                        }
                        break;

                    case Action.ActionType.Emitter:

                        if (Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] is OpenTile)
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new Emitter(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2, new PropagationState[] { PropagationState.Right/*, PropagationState.Left*/}, 800);
                        }
                        else
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new OpenTile(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2);
                        }

                        break;
                }

                usedCoords.Add(dequeuedAction.Coord);
            }
            
            #endregion

            //Updates each tile
            for (int height = 0; height < gridHeight; height++)
            {
                for (int width = 0; width < gridWidth; width++)
                {
                    Grid[height, width].Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                }
            }

            GlobalUpdateId += 1;
            updateTimer.ResetTimer(true);
        }

        #endregion

        if(IsMouseButtonPressed(LEFT_MOUSE, mouse) && IsMouseButtonReleased(LEFT_MOUSE, prevMouse))
        {
            Tuple<byte, byte> coord = MousePosToCoord(mouse);

            if(coord.Item1 >= 0 && coord.Item1 <= gridHeight 
                && coord.Item2 >= 0 && coord.Item2 <= gridWidth)
            {
                pending.Enqueue(new Action(coord, Action.ActionType.Waves));
            }
        }
        else if (IsMouseButtonPressed(RIGHT_MOUSE, mouse) && IsMouseButtonReleased(RIGHT_MOUSE, prevMouse))
        {
            Tuple<byte, byte> coord = MousePosToCoord(mouse);

            if (coord.Item1 >= 0 && coord.Item1 <= gridHeight
                && coord.Item2 >= 0 && coord.Item2 <= gridWidth)
            {
                pending.Enqueue(new Action(coord, Action.ActionType.AbsorbWall));
            }
        }
        else if (IsMouseButtonPressed(MIDDLE_MOUSE, mouse) && IsMouseButtonReleased(MIDDLE_MOUSE, prevMouse))
        {
            Tuple<byte, byte> coord = MousePosToCoord(mouse);

            if (coord.Item1 >= 0 && coord.Item1 <= gridHeight
                && coord.Item2 >= 0 && coord.Item2 <= gridWidth)
            {
                pending.Enqueue(new Action(coord, Action.ActionType.Emitter));
            }
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();

        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {

                if (Superpositions[i,j] == AbsorbWall.SUPERPOSITION)
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.Yellow);
                }
                else if(Grid[i, j] is Emitter)
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.Purple);
                }
                else
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.White * Superpositions[i, j]);
                }
            }
        }

        spriteBatch.Draw(tileImg, uiBgRect, Color.Brown);

        spriteBatch.DrawString(msgFont, INSTRUCTIONS_TEXT, instructionsTextPos, Color.White);

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void AddTwoPointSourceInterference()
    {
        ClearGrid();
        
        Grid[9, 0] = new Emitter(9, 0, new PropagationState[] { PropagationState.Right }, 1000);
        Grid[15, 0] = new Emitter(15, 0, new PropagationState[] { PropagationState.Right }, 1000);

        for (byte i = 0; i < 0 + 23; i++)
        {
            Grid[i, 13] = new AbsorbWall(i, 13);
        }

        for (byte i = 0; i < gridWidth; i++)
        {
            Grid[8, i] = new AbsorbWall(8, i);
        }

        for (byte i = 0; i < gridWidth; i++)
        {
            Grid[16, i] = new AbsorbWall(16, i);
        }
    }

    private void AddDoubleSlit()
    {
        ClearGrid();

        Grid[12, 7] = new Emitter(12, 7, new PropagationState[] { PropagationState.Right }, 500);

        for (byte i = 5; i < 5 + 6; i++)
        {
            Grid[i, 12] = new AbsorbWall(i, 12);
        }

        for (byte i = 14; i < 13 + 6; i++)
        {
            Grid[i, 12] = new AbsorbWall(i, 12);
        }

        for (byte i = 1; i < 1 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
        }

        for (byte i = 9; i < 9 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
        }

        for (byte i = 17; i < 17 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
        }
    }

    //Pre: The mouse button number and the state of the mouse
    //Post: If the mouse button is pressed
    //Desc: Checks if a sepcific mouse button is pressed
    private static bool IsMouseButtonPressed(byte index, MouseState mouse)
    {
        //Chooses which mouse button to check based on the index
        switch (index)
        {
            case LEFT_MOUSE:
                //Returns if the left mouse button is pressed
                return mouse.LeftButton == ButtonState.Pressed;

            case RIGHT_MOUSE:
                //Returns if the right mouse button is pressed
                return mouse.RightButton == ButtonState.Pressed;

            case MIDDLE_MOUSE:
                //Returns if the right mouse button is pressed
                return mouse.MiddleButton == ButtonState.Pressed;

            default:
                //Throws an exception if no valid index was given
                throw new EntryPointNotFoundException("Invalid index passed.");
        }
    }

    private static bool IsMouseButtonReleased(byte index, MouseState mouse)
    {
        //Chooses which mouse button to check based on the index
        switch (index)
        {
            case LEFT_MOUSE:
                //Returns if the left mouse button is released
                return mouse.LeftButton == ButtonState.Released;

            case RIGHT_MOUSE:
                //Returns if the right mouse button is released
                return mouse.RightButton == ButtonState.Released;

            case MIDDLE_MOUSE:
                //Returns if the right mouse button is released
                return mouse.MiddleButton == ButtonState.Released;

            default:
                //Throws an exception if no valid index was given
                throw new EntryPointNotFoundException("Invalid index passed.");
        }
    }

    private static Tuple<byte, byte> MousePosToCoord(MouseState mouse)
    {
        return new Tuple<byte, byte>((byte)(mouse.Y / Tile.DIMENSION), (byte)(mouse.X / Tile.DIMENSION));
    }

    //Pre: The key to check the state of
    //Post: A boolean representing if it was or was not pressed
    //Desc: Checks to see if a key was just pressed
    private bool NewKeyPress(Keys keyToCheck)
    {
        return kb.IsKeyDown(keyToCheck) && !prevKb.IsKeyDown(keyToCheck);
    }
}