using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Cloud.Windows
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public List<HelpPage> HelpPages { get; set; }
        public HelpPage CurrentPage { get; set; }
        public Welcome()
        {
            InitializeComponent();
            HelpPages = new List<HelpPage>()
            {
                new HelpPage()
                {
                    Name = "Welcome",
                    Title = "Welcome to Game Cloud!",
                    Content = String.Format("Thank you for downloading Game Cloud!  The following pages will give you an overview of how Game Cloud works and an explanation of some of its features.{0}{0}You can open this window at any time via the menu.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Overview",
                    Title = "Game Cloud Overview",
                    Content = String.Format("Game Cloud uses a community-made database that contains information about where games keep their save files.  This information is added by users via the New Game tab, which is where you add games that aren't already in the database.{0}{0}When you add a game to your sync list, Game Cloud uses the information about where the game's save files are kept to find your specific files on your computer and sync them to your account.  No actual files are shared in the database, only information about where to find them.{0}{0}In additional to this tutorial, many elements have helpful tooltips when you hover over them, so be sure to read those too.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Overview",
                    Title = "Main Tabs Overview",
                    Content = String.Format("Here's a brief explanation of the purpose of each tab.{0}{0}Synced Games - View, resync, and remove your synced games.{0}{0}Game Database - View, rate, and sync games from the database.{0}{0}New Game - Add details about a game to the database and sync it.{0}{0}Ask for Help - Ask the community for help, such as where a game's save files are.  Or answer other people's questions.{0}{0}Chat - Chat in real-time with other Game Cloud users.{0}{0}Feedback - Send me your ideas or bug reports.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Synced Games",
                    Title = "Tabs - Synced Games",
                    Content = String.Format("After syncing a game from the database, or from the New Game tab, it will show up here.  Each game will contain the information that was entered in the New Game tab, as well as the current sync status and the total size of all its synced files.{0}{0}The Status column contains a symbol indicating the sync state.  When you click on a row, it shows you a description of the status below it.  The next tutorial page also has a legend of each symbol.{0}{0}You can select multiple rows/games by holding Ctrl or Shift while clicking.  You can also select all with Ctrl + A.  You can sync multiple games at once.{0}{0}There are additional options if you right-click a row.  Open Folder will open the location of the save files in Windows Explorer so you can browse it.  Force Update is explained below.{0}{0}When game files are added or updated, the next sync will cause them to upload to your account.  If you delete a file, though, the next sync will cause it to be downloaded again.  I did this intentionally to avoid any accidental deletions.  In my tests, there were rare edge cases where it could happen.  Instead, if you want to delete a file instead of re-download it from the server, right-click and choose Force Update.  This will overwrite everything on the server with the files that are currently on your computer.{0}{0}Lastly, you can use Import and Export to save your synced games list to a file and load them onto another account.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Status Symbols",
                    Title = "Status Symbols",
                    Content = String.Format("Below are a list of the status symbols and their meanings.{0}{0}❓ = The path where the save files are supposed to be wasn't found on your computer.{0}{0}✔ = All files are synced with the server.{0}{0}☁⬇ = There are new or updated files on the server to download.{0}{0}☁⬆ = There are new or updated files on your computer to upload.{0}{0}☁⬇⬆ = There are files for both download and upload.{0}{0}⚠ = A sync failure occurred, such as not being able to save a downloaded file.{0}{0}⛔ = The folder path couldn't be determined.  This can happen if required software isn't installed, such as Steam.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Game Database",
                    Title = "Tabs - Game Database",
                    Content = String.Format("The Known Games list contains the game information that has been added by the community.  You can scroll through it or type into the box, which has auto-complete.  The check box below it will filter the games so that only the ones installed on your computer will be visible.  It determines this by checking if the folder path exists, so if it's filtering out a game that really is installed, you may need to save a character/campaign/etc. first to create the folder.{0}{0}Details for the selected game will display on the right.  Below that, you can rate the game information based on its accuracy.  Games that fall below an overall 0 rating will be removed from the database.  This way, the database is kept free of inaccurate information.{0}{0}If you add a game from here, Game Cloud will use the information to find the save files on your computer for this game and sync them to your account.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - New Game",
                    Title = "Tabs - New Game",
                    Content = String.Format("Here, you can add a game that's not in the database to your sync list.  By default, it will add the information to the online database, but you can uncheck the option if you don't want it to.{0}{0}The Game Name is, you guessed it, the game's title.  The platform is the version of the game, such as the online store from where it was purchased (Steam, GOG, etc.) or any other detail that might change where the game's files are stored.  You can select from a list or type directly in the box.{0}{0}Game's Save Folder is the full path to the folder that contains the save files.  If you can't find the folder for your game, you can use the Ask for Help tab to ask the community, or you can try searching online.{0}{0}The optional File Filter will let you specify a filter to apply to file names within that folder to include or exclude.  Wild cards are automatically used on either side of the search text, so you can use partial phrases.  The file extension is included.  For example, if you chose \"Include only\" and the term \".dat\", only files with the .dat extension will be synced.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Ask for Help",
                    Title = "Tabs - Ask for Help",
                    Content = String.Format("If you need help with a particular game, you can ask the community on this tab.  In order to receive replies via email, you need to set your email in the Account Settings, which is in the menu.{0}{0}This is also where you can reply to questions asked by other users.  Multiple replies can be sent to the same question.  By default, you'll see a number next to the Ask a Question tab showing the number of unanswered questions waiting.  You can turn this feature off in the Settings window.{0}{0}After a question is answered, it will remain in the database for 6 months before being removed automatically.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Chat",
                    Title = "Tabs - Chat",
                    Content = String.Format("The chat system can be used for matchmaking, socializing, or whatever else you'd like.  Just be sure to keep it clean.  It doesn't connect until you open the Chat tab, and it will stay connected while Game Cloud is open.  If you're viewing a different tab, the Chat tab will display a notification when new messages are received.{0}{0}The Upload File button at the bottom right can be used to share files, which will be sent to the chat in the form of a download link.{0}{0}The last 50 chat messages are kept on the server and will be loaded when you sign into chat.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Tabs - Feedback",
                    Title = "Tabs - Feedback",
                    Content = String.Format("Feel free to send me your feedback and bug reports!{0}{0}Also, wow!  I can't believe you've read this far into the tutorial!  Thank you for your interest in Game Cloud.  Send me an email to let me know that you read this message, and I'll increase your storage to 5GB!", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Menu - Account Settings",
                    Title = "Menu - Account Settings",
                    Content = String.Format("Email - Your email address can be used to reset your password or have responses to your help requests emailed to you.  It will never be used for anything else.{0}{0}Password Change - Change your login password.{0}{0}Account Recovery Options - These are the ways you can recover your account in case you forget your password.  Using your machine's GUID will allow you to recover your account from the computer you're currently using.  The unique ID of your computer is used for authentication.  Reset via email will allow you to have a new password emailed to you.  Lastly, you can set a challenge question to answer.  You can set any or all options.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Menu - Options",
                    Title = "Menu - Options",
                    Content = String.Format("Create Local Backups - With this enabled, every time you sync a game, a backup will be made of the current files for that game.  This will help you undo any accidental overwrites.{0}{0}Show Help Requests - When this is enabled, a notification will appear on the Ask for Help tab that shows the number of unanswered questions.{0}{0}Maximum Backup Size - This is the total size of all backup files that will be kept before it starts deleting the oldest ones to make room for new ones.{0}{0}Maximum Backup Files - The total number of backup files to create before removing the old ones, if the maximum backup size isn't reached first.{0}{0}Browse Backup Folder - Opens the folder where the backups are saved.{0}{0}Download Saved Games - This allows you to download all of a game's files stored on the server in one ZIP file.{0}{0}Remove Game Cloud - Removes all the files associated with Game Cloud.", Environment.NewLine)
                },
                new HelpPage()
                {
                    Name = "Miscellaneous - ScummVM",
                    Title = "Miscellaneous - ScummVM",
                    Content = String.Format("Below is a tutorial provided by a Game Cloud user describing the optimal way to backup save files on ScummVM.{0}{0}{0}In each computer where save games are to be synced, create a folder in the root directory (C:), which will contain all the necessary files for the game. Put those files in it, then create a folder named \"SAVES\".{0}{0}Then, in the SCUMMVM options section (BEFORE adding games), make sure that the \"saves\" and \"extra\" options in the \"path\" tab HAVE THE SAME PATH.{0}{0}The default paths are: C:\\Program Files\\ScummVM\\ (for 32-Bit Windows), C:\\Program Files (x86)\\ScummVM\\ (for 64-Bit Windows){0}{0}You can use your own path depending on where you have installed the software (if you have changed the default path), but it is important that the \"saves\" and \"extras\" lead to the same location on you hard disk.{0}{0}Then, add every game you want to play with ScummVM using the \"add\" option of the software. FOR EACH GAME ADDED, use the \"edit\" button and in the \"path\" tab, enter the path of the \"SAVES\" folder you have created within the game folder at the root of your hard disk. Once again, it is important that the \"Saves\" and \"Extra\" option lead to the SAME FOLDER (c:\\[name_of_the_game]\\SAVES). The game path is: C:\\[name_of_the_game].{0}{0}Finally, in Game Cloud, enter the path of the SAVES directory for each game you want to synchronize. You're done!", Environment.NewLine)
                }
            };
            CurrentPage = HelpPages[0];
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            textTitle.Text = CurrentPage.Title;
            textContent.Text = CurrentPage.Content;
        }
        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            var index = HelpPages.IndexOf(CurrentPage);
            if (index > 0)
            {
                CurrentPage = HelpPages[index - 1];
                textTitle.Text = CurrentPage.Title;
                textContent.Text = CurrentPage.Content;
                comboJumpTo.SelectedIndex = index - 1;
                scrollContent.ScrollToTop();
            }
        }
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            var index = HelpPages.IndexOf(CurrentPage);
            if (index < HelpPages.Count - 1)
            {
                CurrentPage = HelpPages[index + 1];
                textTitle.Text = CurrentPage.Title;
                textContent.Text = CurrentPage.Content;
                comboJumpTo.SelectedIndex = index + 1;
                scrollContent.ScrollToTop();
            }
        }
        private void comboJumpTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = (e.AddedItems[0] as HelpPage);
            textTitle.Text = CurrentPage.Title;
            textContent.Text = CurrentPage.Content;
            scrollContent.ScrollToTop();
        }
    }

    public class HelpPage
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
