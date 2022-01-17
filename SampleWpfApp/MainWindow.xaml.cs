using FreeSql.DataAnnotations;
using System.Windows;

namespace SampleWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var repo = DB.Instance.GetRepository<User>();
            User user = repo.Insert(new User() { Name = "12" });
            MessageBox.Show("Add user id:" + user.Id, "MESSAGE");

        }
    }

    public class User
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
