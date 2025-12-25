using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Data;
using System.Collections.ObjectModel;


namespace MTG
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// [+Done+] Features to add: Update comparison, what was the previous Entry And what is the New one, If No data was change then send a message to say nothing changed.
	/// [+Done+] Features to add: If you save the same data the program must inform the user that this card exists and the program must not save the second entry of the same card.
	/// [+Done+] Features to add: Add a total card count to the bottom of the program.
	/// [+Done+] Features to add: Add a message box to confirm deletion of a card.
	/// [+Done+] Features to add: In the save button, if any of the fields are empty or the wrong data type, the program must inform the user that all fields must be filled in or the user used the wrond data type.
	public class Card
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Card_Colour { get; set; }
        public string Card_Rarity { get; set; }
        public string Card_Mana_Cost { get; set; }
        public string Card_Type { get; set; }
        public string Card_Edition { get; set; }
        public int Quantity { get; set; }
	}
	public partial class MainWindow : Window
    {

		//private string dbPath = "Data Source=\"C:\\Users\\Hetze\\source\\repos\\MTGCARDDB\\MTG\\DataBase\\Deck.db\";Version=3;";

		private readonly string dbPath =
	$"Data Source={System.IO.Path.Combine(
		AppDomain.CurrentDomain.BaseDirectory,
		"DataBase",
		"Deck.db"
	)};Version=3;";



		private int selectedCardId = -1;

        private ObservableCollection<Card> cards {get; set;} = new ObservableCollection<Card>();   

		public MainWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = cards;
			LoadCards();
        }

        private void LoadCards()
        {
            cards.Clear();
			using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string query = "SELECT Id, Name, Card_Colour, Card_Rarity, Card_Mana_Cost, Card_Type, Card_Edition, Quantity FROM Cards";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(new Card
                            {
                                Id = reader.GetInt32(0),
								Name = reader.GetString(1),
                                Card_Colour = reader.GetString(2),
                                Card_Rarity = reader.GetString(3),
                                Card_Mana_Cost = reader.GetString(4),
                                Card_Type = reader.GetString(5),
                                Card_Edition = reader.GetString(6),
                                Quantity = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
							});
                        }
                    }
                   
				}

			}
			UpdateCardCount();
		}

        private void Click_Save(object sender, RoutedEventArgs e)
        {
			//Trimmed text inputs for cleaner validation
            string nameInput = txtName.Text.Trim();
            string colourInput = txtColour.Text.Trim(); 
            string rarityInput = txtRarity.Text.Trim();
            string manaCostInput = txtManaCost.Text.Trim();
            string typeInput = txtType.Text.Trim();
            string editionInput = txtEdition.Text.Trim();
            string quantityInput = txtQuantity.Text.Trim();

            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(Name) &&
                string.IsNullOrWhiteSpace(colourInput) &&
                string.IsNullOrWhiteSpace(rarityInput) &&
                string.IsNullOrWhiteSpace(manaCostInput) &&
                string.IsNullOrWhiteSpace(typeInput) &&
                string.IsNullOrWhiteSpace(editionInput) &&
                string.IsNullOrWhiteSpace(quantityInput))
            {
                MessageBox.Show("All fields must be filled in.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
			}

            // Validate Quantity as an integer
            if (!int.TryParse(quantityInput, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Quantity must be a non-negative integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Check for duplicate card by Name and Edition
            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string checkQuery = "SELECT COUNT(*) FROM Cards WHERE Name=@Name AND Card_Edition=@Card_Edition";
                using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Name", nameInput);
                    checkCmd.Parameters.AddWithValue("@Card_Edition", editionInput);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("This card already exists in the database.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
			// Validate Quantity as an integer
            if (!int.TryParse(quantityInput, out int quntity))
            {
                MessageBox.Show("Quantity must be a valid integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
			}

            // Display a summary of the card data before saving
            string cardSummary = $"You are about to save the following card:\n\n" +
                                 $"Name: {nameInput}\n" +
                                 $"Colour: {colourInput}\n" +
                                 $"Rarity: {rarityInput}\n" +
                                 $"Mana Cost: {manaCostInput}\n" +
                                 $"Type: {typeInput}\n" +
                                 $"Edition: {editionInput}\n" +
                                 $"Quantity: {quantityInput}\n\n" +
                                 $"Do you want to proceed?";
            MessageBoxResult result = MessageBox.Show(cardSummary, "Confirm Save", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return; // User chose not to proceed
			}

			using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                
               
                string query = "INSERT INTO Cards (Name, Card_Colour, Card_Rarity, Card_Mana_Cost, Card_Type, Card_Edition, Quantity) VALUES (@Name, @Card_Colour, @Card_Rarity, @Card_Mana_Cost, @Card_Type, @Card_Edition, @Quantity)";
                
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", txtName.Text);
                    cmd.Parameters.AddWithValue("@Card_Colour", txtColour.Text);
					cmd.Parameters.AddWithValue("@Card_Rarity", txtRarity.Text);
					cmd.Parameters.AddWithValue("@Card_Mana_Cost", txtManaCost.Text);
                    cmd.Parameters.AddWithValue("@Card_Type", txtType.Text);
                    cmd.Parameters.AddWithValue("@Card_Edition", txtEdition.Text);
                    cmd.Parameters.AddWithValue("@Quantity", txtQuantity.Text);
					//cmd.Parameters.AddWithValue("@Id", selectedCardId);
					
                    cmd.ExecuteNonQuery();
                }
            }
            ClearForm();
            LoadCards();
            UpdateCardCount();
		}

        private void ClearForm()
        {
            txtName.Text = "";
            txtManaCost.Text = "";
            txtColour.Text = "";
			txtType.Text = "";
            txtRarity.Text = "";
            txtEdition.Text = "";
            txtQuantity.Text = "";
            txtSearch.Text = "";
			selectedCardId = -1;
        }

        private void Click_Update(object sender, RoutedEventArgs e)
        {
            if (selectedCardId == -1)
            {
                MessageBox.Show("Select a card to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Find the card in memory
            var card = cards.FirstOrDefault(c => c.Id == selectedCardId);
            if (card == null)
            {
                MessageBox.Show("Selected card not found in memory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build a list of changes for comparison
            var changes = new List<string>();

			void Compare(string field, string oldValue, string newValue)
			{
				if (oldValue != newValue)
					changes.Add($"{field}: '{oldValue}' -> '{newValue}'");
			}

			// ✅ Now call the comparisons here:
			Compare("Name", card.Name, txtName.Text);
			Compare("Colour", card.Card_Colour, txtColour.Text);
			Compare("Rarity", card.Card_Rarity, txtRarity.Text);
			Compare("Mana Cost", card.Card_Mana_Cost, txtManaCost.Text);
			Compare("Type", card.Card_Type, txtType.Text);
			Compare("Edition", card.Card_Edition, txtEdition.Text);
			Compare("Quantity", card.Quantity.ToString(), txtQuantity.Text);



			// If no changes, inform the user and exit
			if (changes.Count == 0)
                {
                    MessageBox.Show("No changes detected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }


                // Proceed with update if there are changes
                using (SQLiteConnection conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();
                    string query = "UPDATE Cards SET Name=@Name, Card_Colour=@Card_Colour, Card_Rarity=@Card_Rarity, " +
                                   "Card_Mana_Cost=@Card_Mana_Cost, Card_Type=@Card_Type, Card_Edition=@Card_Edition, " +
                                   "Quantity=@Quantity WHERE Id=@Id";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@Card_Colour", txtColour.Text);
                        cmd.Parameters.AddWithValue("@Card_Rarity", txtRarity.Text);
                        cmd.Parameters.AddWithValue("@Card_Mana_Cost", txtManaCost.Text);
                        cmd.Parameters.AddWithValue("@Card_Type", txtType.Text);
                        cmd.Parameters.AddWithValue("@Card_Edition", txtEdition.Text);
                        cmd.Parameters.AddWithValue("@Quantity", int.TryParse(txtQuantity.Text, out int qty) ? qty : 0);
                        cmd.Parameters.AddWithValue("@Id", selectedCardId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Update in-memory object
                card.Name = txtName.Text;
                card.Card_Colour = txtColour.Text;
                card.Card_Rarity = txtRarity.Text;
                card.Card_Mana_Cost = txtManaCost.Text;
                card.Card_Type = txtType.Text;
                card.Card_Edition = txtEdition.Text;
                card.Quantity = int.TryParse(txtQuantity.Text, out int newQtyParsed) ? newQtyParsed : card.Quantity;

                dataGrid.Items.Refresh();

                // Show summary of changes
                string changeSummary = string.Join("\n", changes);
                MessageBox.Show($"Update successful.\n\nChanges:\n{changeSummary}", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
			    UpdateCardCount();
		}
		







		private void Click_Delete(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is Card SelectedCard)
            {
                
                using (SQLiteConnection conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();
                    string query = "DELETE FROM Cards WHERE Id=@Id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", SelectedCard.Id);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Card deleted successfully.", "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
					}
                }
                // Find the Card object in the ObservableCollection and remove it
                var cardToRemove = cards.FirstOrDefault(c => c.Id == SelectedCard.Id);
                if (cardToRemove != null)
                {
                    cards.Remove(cardToRemove);
                }
            }
        }
        private void Click_Clear(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is Card card)
            {
                selectedCardId = card.Id;
				txtName.Text = card.Name;
				txtColour.Text = card.Card_Colour;
                txtRarity.Text = card.Card_Rarity;
                txtManaCost.Text = card.Card_Mana_Cost;
                txtType.Text = card.Card_Type;
                txtEdition.Text = card.Card_Edition;
                txtQuantity.Text = card.Quantity.ToString();
			}
        }

        private void Click_Help(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MTG Card Manager\nVersion 1.0\nDeveloped by Hetzer", "About", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Click_Search(object sender, RoutedEventArgs e)
		{
            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string query = "SELECT Id, Name, Card_Colour, Card_Rarity, Card_Mana_Cost, Card_Type, Card_Edition, Quantity " +
                    "FROM Cards WHERE Name LIKE @Search OR Card_Colour LIKE @Search OR Card_Rarity LIKE @Search " +
                    "OR Card_Mana_Cost LIKE @Search OR Card_Type LIKE @Search OR Card_Edition LIKE @Search " +
                    "OR CAST(Quantity AS TEXT) LIKE @Search";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + txtSearch.Text + "%");
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        cards.Clear();
                        while (reader.Read())
                        {
                            cards.Add(new Card
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Card_Colour = reader.GetString(2),
                                Card_Rarity = reader.GetString(3),
                                Card_Mana_Cost = reader.GetString(4),
                                Card_Type = reader.GetString(5),
                                Card_Edition = reader.GetString(6),
                                Quantity = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
							});
                        }
                    }
                }
			}

            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                LoadCards();
                return;
			}
		}

        private void UpdateCardCount()
        {
            int totalCards = 0;

            using (SQLiteConnection conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string query = "SELECT SUM(Quantity) FROM Cards";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        totalCards = Convert.ToInt32(result);
                    }
                }
            }

            lblTotalCards.Text = $"Total Cards: {totalCards}";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
            //This is the update button that will use the velopack updater
            
		}
	} // <-- This closes MainWindow class
} // <-- This closes namespace MTG