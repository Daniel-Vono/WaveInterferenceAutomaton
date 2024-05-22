// File Name: Game1.cs
// Creation Date: Apr. 29, 2024
// Description: The driver class

using GameUtility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{

    private static byte gridWidth = 48;
    private static byte gridHeight = 32;

    private const int UI_WIDTH = 128;
    private static readonly int SCREEN_WIDTH = gridWidth * Tile.DIMENSION + UI_WIDTH;
    private static readonly int SCREEN_HEIGHT = gridHeight * Tile.DIMENSION;

    private static readonly Rectangle uiBgRect = new Rectangle(gridWidth * Tile.DIMENSION, 0, UI_WIDTH, SCREEN_HEIGHT);

    const int LEFT_MOUSE = 0;
    const int RIGHT_MOUSE = 1;
    const int MIDDLE_MOUSE = 2;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D tileImg;

    public static Tile[,] Grid { get; private set; }

    public static byte GlobalUpdateId { get; private set; }

    private Timer updateTimer;

    private MouseState mouse;
    private MouseState prevMouse;

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

        updateTimer = new Timer(800, true);
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


        Grid = new Tile[gridHeight, gridWidth];
        for (byte row = 0; row < gridHeight; row++)
        {
            for (byte column = 0; column < gridWidth; column++)
            {
                Grid[row, column] = new OpenTile(row, column);
            }
        }

        Grid[17, 7] = new Emitter(17, 7, new PropagationState[] { PropagationState.Right}, 200);

        for (byte i = 11; i < 11 + 5; i++)
        {
            Grid[i, 12] = new AbsorbWall(i, 12);
        }

        for (byte i = 19; i < 19 + 5; i++)
        {
            Grid[i, 12] = new AbsorbWall(i, 12);
        }

        for (byte i = 7; i < 7 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
        }

        for (byte i = 15; i < 15 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
        }

        for (byte i = 23; i < 23 + 5; i++)
        {
            Grid[i, 16] = new AbsorbWall(i, 16);
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
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        updateTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

        if (updateTimer.IsFinished())
        {
            usedCoords = new List<Tuple<byte, byte>>();

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
                        }
                        else
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new OpenTile(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2);
                        }
                        break;

                    case Action.ActionType.Emitter:

                        if (Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] is OpenTile)
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new Emitter(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2, new PropagationState[] { PropagationState.Right, PropagationState.Left}, 800);
                        }
                        else
                        {
                            Grid[dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2] = new OpenTile(dequeuedAction.Coord.Item1, dequeuedAction.Coord.Item2);
                        }

                        break;
                }

                usedCoords.Add(dequeuedAction.Coord);
            }


            // TODO: Add your update logic here
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

        prevMouse = mouse;
        mouse = Mouse.GetState();

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

        float opacity = 0f;

        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                opacity = Grid[i, j].Superposition();

                if (opacity == AbsorbWall.SUPERPOSITION)
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.Yellow);
                }
                else if(Grid[i, j] is Emitter)
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.Purple);
                }
                else
                {
                    spriteBatch.Draw(tileImg, Grid[i, j].Rect, Color.White * opacity);
                }
            }
        }

        spriteBatch.Draw(tileImg, uiBgRect, Color.Brown);

        spriteBatch.End();

        base.Draw(gameTime);
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
}