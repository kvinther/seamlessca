using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
// ReSharper disable LocalizableElement

namespace CA.Form
{

    public partial class MainForm : System.Windows.Forms.Form
    {
        private World _world;
        private Point _location;
        private bool _drawGrid;
        private bool _drawEdges = true;
        private readonly CaProperties _properties;
        private bool _drawView = true;
        private HelpForm _helpForm;
        private bool _drawEdgesAsNoise;
        private bool _drawFinalAsNoise;
        private bool _colorizeInsideChunks = true;

        public MainForm()
        {
            InitializeComponent();

            KeyPreview = true;
            KeyDown += OnKeyDown;


            _properties = new CaProperties();
            _world = BraveNewWorld();

            var dim = 10*_properties.ChunkSize;

            Size = new Size(1024, 800);

            propertyGrid1.SelectedObject = _properties;
            propertyGrid1.PropertyValueChanged += (o, args) =>
            {
                
                switch (args.ChangedItem.Label)
                {
                    
                    case "MovementFactor":
                        _world.MovementFactor = _properties.MovementFactor;
                        break;    
                    case "CellSize":
                        _world.CellSize = _properties.CellSize;
                        break;
                    //case "ViewPortSize":
                    //    _world.ViewPortSize = _properties.CellSize;
                    //    break;
                    default:
                        BraveNewWorld();
                        DrawImage();
                        break;
                }
            };

            DrawImage();

            
            pictureBox1.Focus();
        }

        private World BraveNewWorld()
        {
            _location = new Point();

            return new World
            {
                CellSize = _properties.CellSize,
                ViewPortSize = _properties.ViewPortSize,
                ChunkSizeY = _properties.ChunkSize,
                ChunkSizeX = _properties.ChunkSize,
                MovementFactor = _properties.MovementFactor,
                RockPercentage = _properties.RockPercentage / 100.0,
                Generations = _properties.Generations,
                WorldSeed = _properties.WorldSeed

            };
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.SuppressKeyPress = true;
            keyEventArgs.Handled = true;
            switch (keyEventArgs.KeyCode)
            {
                case Keys.Left:
                    _location.X = _location.X - _world.MovementFactor;
                    break;
                case Keys.Right:
                    _location.X = _location.X + _world.MovementFactor;
                    break;
                case Keys.Up:
                    _location.Y = _location.Y - _world.MovementFactor;
                    break;
                case Keys.Down:
                    _location.Y = _location.Y + _world.MovementFactor;
                    break;
                case Keys.R:
                    _world = BraveNewWorld();
                    break;
                case Keys.G:
                    _drawGrid = !_drawGrid;
                    break;
                case Keys.V:
                    _drawView = !_drawView;
                    break;
                case Keys.E:
                    _drawEdges = !_drawEdges;
                    break;
                case Keys.S:
                    SaveImage();
                    break;
                case Keys.N:
                    _drawEdgesAsNoise = !_drawEdgesAsNoise;
                    break;
                case Keys.F:
                    _drawFinalAsNoise = !_drawFinalAsNoise;
                    break;
                case Keys.Escape:
                    Close();
                    break;
                case Keys.F1:
                    _helpForm?.Close();
                    _helpForm = new HelpForm();
                    _helpForm.Show();
                    break;
                case Keys.Space:
                    _colorizeInsideChunks = !_colorizeInsideChunks;
                    break;
                default:
                    keyEventArgs.SuppressKeyPress = false;
                    keyEventArgs.Handled = false;
                    break;
            }

            DrawImage();
        }

        private Image GenerateImage()
        {
            var viewPort = _world.RenderViewPort(_location);
            return _world.DrawImage(viewPort, _drawEdges, _drawGrid, _drawView, _drawEdgesAsNoise, _drawFinalAsNoise, _colorizeInsideChunks);
        }

        private void SaveImage()
        {
            var directoryInfo = new DirectoryInfo(_properties.SaveFileFolder);
            if (directoryInfo.Exists)
            {
                var image = GenerateImage();
                var fileName = Path.ChangeExtension(DateTime.Now.ToString("s"), "png").Replace(":", "_");
                image.Save(Path.Combine(directoryInfo.FullName, fileName), ImageFormat.Png);
            }
            else
            {
                MessageBox.Show(
                    this,
                    $"Directory not found: {directoryInfo.FullName}. Please create it.",
                    "Directory not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DrawImage()
        {
            pictureBox1.Image = GenerateImage();
        }
    }
}
