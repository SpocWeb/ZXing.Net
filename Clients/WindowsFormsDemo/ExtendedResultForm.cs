using System.Windows.Forms;

using ZXing.Client.Result;

namespace WindowsFormsDemo
{
    public partial class ExtendedResultForm : Form
    {
        public ParsedResult Result
        {
            get => (ParsedResult)propGridResult.SelectedObject;
            set => propGridResult.SelectedObject = value;
        }

        public ExtendedResultForm()
        {
            InitializeComponent();
        }
    }
}
