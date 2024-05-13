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
    private readonly static int SCREEN_WIDTH = gridWidth * Tile.DIMENSION + UI_WIDTH;
    private readonly static int SCREEN_HEIGHT = gridHeight * Tile.DIMENSION;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D tileImg;

    public static Tile[,] Grid { get; private set; }

    public static byte GlobalUpdateId { get; private set; }

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
            /*if(i % Tile.DIMENSION == 0)
            {
                data[i] = new Color(0, 0, 0, 255);
            }
            else
            {
                data[i] = new Color(205, 0, 0, i);
            }*/

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

        Grid[16, 0].AddParticle(PropagationState.Right, 10, GlobalUpdateId);
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

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            // TODO: Add your update logic here
            for (int height = 0; height < gridHeight; height++)
            {
                for (int width = 0; width < gridWidth; width++)
                {
                    Grid[height, width].Update();
                }
            }

            GlobalUpdateId += 1;
        }



        base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin();

        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                spriteBatch.Draw(tileImg, Grid[i,j].Rect, Color.White * Grid[i,j].Superposition());
            }
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }
}