using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;

namespace CSharpXnaUsingAsync
{
   public class GameAsync : Game
   {
      readonly GraphicsDeviceManager graphicsDeviceManager;
      readonly TaskCompletionSource<object> initializeTCS = new TaskCompletionSource<object>();
      readonly TaskCompletionSource<object> loadContentTCS = new TaskCompletionSource<object>();
      TaskCompletionSource<GameTime> updateTask = new TaskCompletionSource<GameTime>();
      TaskCompletionSource<GameTime> drawTask = new TaskCompletionSource<GameTime>();

      public GameAsync()
      {
         graphicsDeviceManager = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }

      protected override void Initialize()
      {
         graphicsDeviceManager.PreferredBackBufferWidth = 640;
         graphicsDeviceManager.PreferredBackBufferHeight = 480;
         graphicsDeviceManager.ApplyChanges();

         base.Initialize();
         initializeTCS.SetResult(null);
      }

      protected override void LoadContent()
      {
         loadContentTCS.SetResult(null);
      }

      protected override void Update(GameTime gameTime)
      {
         updateTask.SetResult(gameTime);
         updateTask = new TaskCompletionSource<GameTime>();
      }

      protected override void Draw(GameTime gameTime)
      {
         drawTask.SetResult(gameTime);
         drawTask = new TaskCompletionSource<GameTime>();
      }

      public Task InitializeTask
      {
         get { return initializeTCS.Task; }
      }

      public Task LoadContentTask
      {
         get { return loadContentTCS.Task; }
      }

      public Task<GameTime> UpdateTask
      {
         get { return updateTask.Task; }
      }

      public Task<GameTime> DrawTask
      {
         get { return drawTask.Task; }
      }
   }

   static class GameWorkflow
   {
      static async void StartWorkflow(GameAsync game) {
         await game.InitializeTask;
         var spriteBatch = new SpriteBatch(game.GraphicsDevice);
         await game.LoadContentTask;
         var sprite = game.Content.Load<Texture2D>("Sprite");
         await Loop(game, spriteBatch, sprite, 0.0f, 0.0f, 4.0f, 4.0f);
      }

      static async Task Loop(GameAsync game, SpriteBatch spriteBatch, Texture2D sprite, float x, float y, float dx, float dy)
      {
         var updateTime = await game.UpdateTask;
         if (x > 608.0f || x < 0.0f) dx = -dx;
         if (y > 448.0f || y < 0.0f) dy = -dy;

         x = x + dx;
         y = y + dy;
         var drawTime = await game.DrawTask;
         game.GraphicsDevice.Clear(Color.CornflowerBlue);
         spriteBatch.Begin();
         spriteBatch.Draw(sprite, new Vector2(x, y), Color.White);
         spriteBatch.End();

         await Loop(game, spriteBatch, sprite, x, y, dx, dy);
      }

      public static IDisposable RunGame()
      {
         var game = new GameAsync();
         StartWorkflow(game);
         game.Run();
         return game;
      }
   }

   static class Program
   {
      static void Main(string[] args)
      {
         using (GameWorkflow.RunGame()) { };
      }
   }
}