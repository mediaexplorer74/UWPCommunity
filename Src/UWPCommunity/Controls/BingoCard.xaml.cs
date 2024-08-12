﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
//using System.Web;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPCommunity.Controls
{
    public sealed partial class BingoCard : UserControl
    {
        static readonly Version BingoVersion = new Version(App.GetVersion());
        static List<string> AllTiles;

        public BingoCard()
        {
            this.InitializeComponent();
            ResetBoard();
        }
        public BingoCard(string dataString, string boardVersion)
        {
            this.InitializeComponent();
            // Create a board from data string
            
        }

        /// <summary>
        /// Loads tiles from file. Does not reset the board.
        /// </summary>
        public async Task InitBoard()
        {
            if (AllTiles != null)
                return;

            // Create an HTTP client object
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();

            // Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri("https://gist.githubusercontent.com/michael-hawker/283fa0ba3577f96e753fde3ac6109618/raw/71f229862e19a60a502af82f3b95a6c9a655f24c/squares.txt");

            // Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";

            try
            {
                // Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                // NEVER change the order of the lines in this file.
                // GetBoardAsDataString() relies on the order being constant,
                // so the line number can be used as a unique ID for each tile.
                AllTiles = httpResponseBody.Split('\n', (char)StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
        }

        public async Task ResetBoard()
        {
            await InitBoard();

            // Clear the board of tiles
            var items = BingoGrid.Children.ToList();
            foreach (UIElement item in items)
            {
                if (item is ToggleButton)
                    BingoGrid.Children.Remove(item);
            }

            // Randomly choose 24 tiles
            var newTiles = GetRandom(AllTiles, 24);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    // Check if the tile is the free space
                    if (x == 2 && y == 2)
                        continue;

                    int boardIndex = 5 * x + y;
                    // Account for free space
                    if (boardIndex > 12)
                        boardIndex--;
                    SetTile(newTiles.ElementAt(boardIndex), x, y, false, false);
                }
            }
            BoardChanged?.Invoke(ToDataString());
        }

        private void SetTile(string text, int x, int y, bool isFilled = false, bool fireBoardChanged = true)
        {
            if (x == 2 && y == 2)
            {
                // Attempting to set the FREE space, ignore the request
                return;
            }

            var tileButton = CreateTile(text, isFilled);

            int boardIndex = 5 * x + y;
            if (boardIndex >= BingoGrid.Children.Count)
                BingoGrid.Children.Add(tileButton);
            else
                BingoGrid.Children.Insert(5 * x + y, tileButton);

            if (fireBoardChanged)
                BoardChanged?.Invoke(ToDataString());
        }

        private void AddTile(string text, bool isFilled = false)
        {
            BingoGrid.Children.Add(CreateTile(text, isFilled));
        }

        private ToggleButton CreateTile(string text, bool isFilled)
        {
            var tileButton = new ToggleButton();
            var tileText = new TextBlock();
            tileText.Style = (Style)Resources["BingoBox"];
            tileText.Text = text;
            tileButton.Content = tileText;
            tileButton.IsChecked = isFilled;
            tileButton.Checked += InvokeBoardChanged;
            tileButton.Unchecked += InvokeBoardChanged;
            return tileButton;
        }

        private (string text, bool isFilled) GetTile(int x, int y)
        {
            var tileButton = BingoGrid.Children[5 * x + y] as ToggleButton;
            if (tileButton == null)
            {
                // This means the button wasn't found. It is likely the FREE tile.
                return ("FREE", true);
            }
            var tileText = tileButton.Content as TextBlock;
            return (tileText.Text, tileButton.IsChecked.Value);
        }
        private (string text, bool isFilled) GetTile(Point p)
		{
            return GetTile((int)p.X, (int)p.Y);
		}

        private Grid GenerateFreeTile()
        {
            //<Grid Background="{ThemeResource SystemAccentColor}"
            //    Grid.Row="2" Grid.Column="2" x:Name="tileFree">
            //    <TextBlock Style="{StaticResource BingoBox}" FontSize="28">
            //    <Run>🦙</Run><LineBreak/><Run>FREE</Run>
            //    </TextBlock>
            //</Grid>
            return new Grid
            {
                Background = (SolidColorBrush)Resources["AccentBrush"],
                Children =
                {
                    new TextBlock
                    {
                        Style = (Style)Resources["BingoBox"],
                        FontSize = 28,
                        Inlines =
                        {
                            new Run
                            {
                                Text = "Free"
                            },

                            new LineBreak(),

                            new Run
                            {
                                Text = "🦙"
                            }
                        }
                    }
                }
            };
        }

        public static Random randomizer = new Random();

        public static IEnumerable<T> GetRandom<T>(IEnumerable<T> list, int numItems)
        {
            return GetRandom(list as T[] ?? list.ToArray(), numItems);
        }

        public static IEnumerable<T> GetRandom<T>(T[] list, int numItems)
        {
            var items = new HashSet<T>(); // don't want to add the same item twice; otherwise use a list
            while (numItems > 0)
                // if we successfully added it, move on
                if (items.Add(list[randomizer.Next(list.Length)])) numItems--;

            return items;
        }

        /// <summary>
        /// Generates a string that can be used to share the current state of the board
        /// with other users of this app
        /// </summary>
        public string ToDataString()
        {
            // NOTE: When making significant changes to this algorithm,
            // don't delete the code. Create an if branch to run the
            // previous version of the algorithm.

            byte[] tileData = new byte[25];
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    // Get the tile and extract serializable data
                    (string Text, bool IsFilled) tile = GetTile(x, y);
                    int tileIndex = AllTiles.IndexOf(tile.Text) + 1;
                    int isFilled = tile.IsFilled ? 1 : 0;

                    // Encode the tile data as a byte and convert to hex.
                    // The first 7 bits represent the tile id,
                    // the last bit represents whether the tile is filled.
                    byte enc = Convert.ToByte(tileIndex << 1);
                    enc += Convert.ToByte(isFilled);
                    tileData[5 * x + y] = enc;
                }
            }
            return BitConverter.ToString(tileData).Replace("-", string.Empty);
        }

        /// <summary>
        /// Sets the current state of the board to the one specified by the data string
        /// </summary>
        public async Task SetByDataString(string dataString, Version boardVersion = null)
        {
            // Check if the version is provided. If not, assume it is the current version.
            if (boardVersion == null)
                boardVersion = BingoVersion;

            await ResetBoard();
            BingoGrid.Children.Clear();

            if (string.IsNullOrWhiteSpace(dataString))
                // There is no data to load
                return;

            // NOTE: When making significant changes to this algorithm,
            // don't delete the code. Create an if branch to run the
            // previous version of the algorithm.

            byte[] tileData = dataString.Replace("\r", String.Empty).Replace("\n", String.Empty).TakeEvery(2)
                .Select(s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var data = tileData[5 * x + y];
                    int textIndex = (data >> 1) - 1;
                    if (textIndex == -1 || (x == 2 && y == 2))
                    {
                        // Tile is free space
                        BingoGrid.Children.Add(GenerateFreeTile());
                        continue;
                    }

                    string text = AllTiles[textIndex];
                    bool isFilled = (data & 1) == 1;
                    AddTile(text, isFilled);
                }
            }
            BoardChanged?.Invoke(dataString);
        }

        public string GetShareLink()
        {
                                                  // &board={HttpUtility.UrlEncode(ToDataString())}
            return $"uwpcommunity://llamabingo?version={BingoVersion}";
        }

        /// <summary>
        /// Checks if the current board has any Llamingos
        /// </summary>
        /// <param name="tiles">A list of the tiles involved in completing the bingo</param>
        public bool HasBingo(out List<Point> tiles)
		{
            int adjacentCount = 0;
            tiles = new List<Point>();

            // Check the edge tiles first. It's impossible to have
            // a bingo without at least two edge tiles filled.

            // Check top and bottom edges
            for (int y = 0; y < 5; y++)
            {
                adjacentCount = 0;
                bool isTopFilled = GetTile(0, y).isFilled;
                bool isBottomFilled = GetTile(4, y).isFilled;

                if (isTopFilled && isBottomFilled)
                {
                    tiles.Add(new Point(0, y));
                    tiles.Add(new Point(4, y));

                    // Check the tiles in-between the edges
                    for (int x = 1; x < 4; x++)
                    {
                        if (GetTile(x, y).isFilled)
                        {
                            tiles.Add(new Point(x, y));
                            adjacentCount++;
                        }
                    }

                    // Look for three adjacent, since we already know that
                    // two of the tiles (the ends) are filled
                    if (adjacentCount >= 3)
                        return true;
                    else tiles.Clear();
                }
            }

            // Check left and right edges
            for (int x = 0; x < 5; x++)
            {
                adjacentCount = 0;
                bool isLeftFilled = GetTile(x, 0).isFilled;
                bool isRightFilled = GetTile(x, 4).isFilled;

                if (isLeftFilled && isRightFilled)
                {
                    tiles.Add(new Point(x, 0));
                    tiles.Add(new Point(x, 4));

                    // Check the tiles in-between the edges
                    for (int y = 1; y < 4; y++)
                    {
                        if (GetTile(x, y).isFilled)
                        {
                            tiles.Add(new Point(x, y));
                            adjacentCount++;
                        }
                    }

                    // Look for three adjacent, since we already know that
                    // two of the tiles (the ends) are filled
                    if (adjacentCount >= 3)
                        return true;
                    else tiles.Clear();
                }
            }

            // Check top-left/bottom-right (descending) diagonal
            bool isTopLeftFilled = GetTile(0, 0).isFilled;
            bool isBottomRightFilled = GetTile(4, 4).isFilled;
            if (isTopLeftFilled && isBottomRightFilled)
            {
                adjacentCount = 0;
                int y;
                for (int x = 1; x < 4; x++)
                {
                    y = x;
                    if (GetTile(x, y).isFilled)
                    {
                        tiles.Add(new Point(x, y));
                        adjacentCount++;
                    }
                }

                // Look for three adjacent, since we already know that
                // two of the tiles (the ends) are filled
                if (adjacentCount >= 3)
                    return true;
                else tiles.Clear();
            }

            // Check top-right/bottom-left (ascending) diagonal
            bool isTopRightFilled = GetTile(0, 4).isFilled;
            bool isBottomLeftFilled = GetTile(4, 0).isFilled;
            if (isTopRightFilled && isBottomLeftFilled)
            {
                adjacentCount = 0;
                for (int x = 1; x < 4; x++)
                {
                    int y = 4 - x;
                    if (GetTile(x, y).isFilled)
                    {
                        tiles.Add(new Point(x, y));
                        adjacentCount++;
                    }
                }

                // Look for three adjacent, since we already know that
                // two of the tiles (the ends) are filled
                if (adjacentCount >= 3)
                    return true;
                else tiles.Clear();
            }

            // If we've gone this far, then there can't be
            // any bingos.
            return false;
        }

        private readonly Dictionary<Point, Direction> DIRECTIONS = new Dictionary<Point, Direction>()
        {
            { new Point(0, 1), Direction.Up },
            { new Point(1, 1), Direction.UpRight },
            { new Point(1, 0), Direction.Right },
            { new Point(1, -1), Direction.DownRight },
            { new Point(0, -1), Direction.Down },
            { new Point(-1, -1), Direction.DownLeft },
            { new Point(-1, 0), Direction.Left },
            { new Point(-1, 1), Direction.UpLeft },
        };
        /// <summary>
        /// Returns all of the directions in which the given tile has
        /// a filled tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private IEnumerable<Direction> HasAdjacent(int x, int y)
		{
            foreach (KeyValuePair<Point, Direction> pair in DIRECTIONS)
			{
                Point d = pair.Key;
                if (GetTile(x + (int)d.X, y + (int)d.Y).isFilled)
                    yield return pair.Value;
			}
		}

        [Flags]
        public enum Direction : byte
		{
            Up          = 0b_1000,
            UpRight     = Up | Right,
            Right       = 0b_0001,
            DownRight   = Down | Right,
            Down        = 0b_0100,
            DownLeft    = Down | Left,
            Left        = 0b_0010,
            UpLeft      = Up | Left,
        }

        public delegate void BoardChangedHandler(string data);
        public event BoardChangedHandler BoardChanged;

        private void InvokeBoardChanged(object sender, RoutedEventArgs e) => BoardChanged?.Invoke(ToDataString());
    }
}
