﻿// File Name: Game1.cs
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

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D tileImg;

    public static Tile[,] Grid { get; private set; }

    public static byte GlobalUpdateId { get; private set; }

    private Timer updateTimer;

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
}