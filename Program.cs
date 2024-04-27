using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;
using System.Diagnostics;
using System.Dynamic;
namespace Html_Analizer
{
    class Program
    {
        static string[] allowedStrings = [
            "<span><br>\\[%digit%\\] ([A-Z0-9 ]+) - </span>",
            "<span style=\"color:#[A-Z0-9]+\"> ([A-Z ]+)(?=(?: \\[%digit%\\])? </span>)",
            "<span><br>\\[%digit%\\] [A-Z0-9 ]+ - </span><span style=\"color:#[A-Z0-9]+\"> ([A-Z ]+)(?: \\[%digit%\\])? </span>",
            "<span><div class=\"tooltipprev\"> ⎗ <span class=\"tooltiptext\"><span style=\"color:#[A-Z0-9]+\"> ([A-Z0-9]+) </span></span></span></div>",
            "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\">(?:<sprite=\"[A-Z]+\" name=\"PlayerNumbers_%digit%\">)?(?:<color=#[A-Z0-9]+>)?)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>",
            "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r45\">)?(?:<sprite=\"RoleIcons\" name=\"Role45\">)?<b><color=#[A-Z0-9]+>Jester</color></b></link></font></style> will get their revenge from the grave!</span>",
            "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r44\">(?:<sprite=\"RoleIcons\" name=\"Role44\">)?<b><color=#[A-Z0-9]+>Executioner</color></b></link></font></style> and left town.</span>",
            "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?[A-Z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r43\">(?:<sprite=\"RoleIcons\" name=\"Role43\">)?<b><color=#[A-Z0-9]+>Doomsayer</color></b></link></font></style> and left town.</span>",
            "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?[A-Z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r46\">(?:<sprite=\"RoleIcons\" name=\"Role46\">)?<b><color=#[A-Z0-9]+>Pirate</color></b></link></font></style> and left town.</span>",
            "<span style=\"background-color:#FF5500; color:#FFFFFF\"><br>(?:[A-Z0-9 ,.!-<=/>\"]+)?<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r253\">(?:<sprite=\"RoleIcons\" name=\"Role253\">)?<b><color=#[A-Z0-9]+>Death</color></b></link></font></style>(?:[A-Z0-9 ,.!-<=/>\"]+)?</span>",
            "<span style=\"background-color:#(FF0000|000000); color:#FFFFFF\"><br>(.+)?They (also )?disconnected from life\\.</span>",
            ];
        static void Main(string[] args)
        {
            Console.WriteLine("Gamelogs analyzer.");
            string name = "";
            string rootdir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location!)!;
            string dir = "";
        initialize:
            Console.WriteLine("please write the name of the folder that contains all the gamelogs. (Must be on the same folder as this program).");
            name = Console.ReadLine()!;
            dir = Path.Combine(rootdir, name);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"path '{dir}' was not found, maybe you misspelled the name?");
                goto initialize;
            }

            //Console.WriteLine($"Do you wish to include BTOS2 roles in the output? type 'y' if yes."); //TODO
            bool btos = false;//Console.ReadLine() == "y";
            string csv = Path.Combine(rootdir, "output.csv");
            if (!File.Exists(csv))
            {

                Console.WriteLine($"Creating output.csv");
                File.Create(csv).Close();
                if (btos)
                {
                    File.WriteAllText(csv, "Date, # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won");
                }
                else
                {
                    File.WriteAllText(csv, "Gamelog, Duration (Days), # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had marshal, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of socialites, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won"/*, Player 1, Player 2, Player 3, Player 4, Player 5, Player 6, Player 7, Player 8, Player 9, Player 10, Player 11, Player 12, Player 13, Player 14, Player 15"*/);
                }
            }
            else
            {
                Console.WriteLine($"output.csv already exists, running this program will delete all info within it, please make a copy of it if you want to keep it (press anything to continue).");
                if (Console.ReadLine() == "y")
                    if (btos)
                    {
                        File.WriteAllText(csv, "Date, # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won");
                    }
                    else
                    {
                        File.WriteAllText(csv, "Gamelog, Duration (Days), # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had marshal, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of socialites, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won"/*, Player 1, Player 2, Player 3, Player 4, Player 5, Player 6, Player 7, Player 8, Player 9, Player 10, Player 11, Player 12, Player 13, Player 14, Player 15"*/);
                    }
            }


            string gamelogs = "";
            string[] files = Directory.GetFiles(dir);
            string corrup = "";
            string skipped = "";
            foreach (string file in files)
            {
                Game game = new();
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    Console.WriteLine($"Skipping file with no name...");
                    continue;
                }
                Console.WriteLine($"Starting to parse {fileName}");
                string htmlFullText = File.ReadAllText(file).ToUpper();
                List<string> html = new();
                foreach (string p in htmlFullText.Split("\n"))
                {
                    bool add = false;
                    foreach (string tryMatch in allowedStrings)
                    {
                        if (!Regex.IsMatch(p, tryMatch.ToUpper().Replace("%DIGIT%", "\\d+"))) continue;
                        add = true;
                        break;
                    }
                    if (!add) continue;
                    html.Add(p.ToUpper());
                }

                try { html[html.Count - 1].Remove(html.Last().Length - 3); }
                catch
                {
                    Console.WriteLine($"File: {fileName} has no content.");
                    continue;
                }
                Console.WriteLine(string.Join("\n", html));
                int index = 0;
                bool con = false;
                int me = 0;
                while (index < html.Count)
                {
                    Match match = Regex.Match(html[index], "<span><br>\\[%digit%\\] ([A-Z0-9 ]+) - </span>(?:[%linebreak%]+)?<span style=\"color:#[A-Z0-9]+\"> ([A-Z ]+)(?=(?: \\[%digit%\\])? </span>)".ToUpper().Replace("%DIGIT%", "\\d+").Replace("%LINEBREAK%", "\\r\\n"));
                    if (!match.Success)
                    {
                        Console.WriteLine("Hello");
                        if (html[index + 1].IsExactMatch("<span style=\"color:#[A-Z0-9]+\"> ([A-Z ]+)(?: \\[%digit%\\])? </span>".ToUpper().Replace("%DIGIT%", "\\d+")))
                        {
                            Console.WriteLine("Hello");
                            match = Regex.Match(html[index] + html[index + 1], "<span><br>\\[%digit%\\] ([A-Z0-9 ]+) - </span>(?:[%linebreak%]+)?<span style=\"color:#[A-Z0-9]+\"> ([A-Z ]+)(?=(?: \\[%digit%\\])? </span>)".ToUpper().Replace("%DIGIT%", "\\d+").Replace("%LINEBREAK%", "\\r\\n"));
                            index++;
                        }
                        else break;
                    }
                    string next = html[index + 1];
                    try
                    {
                        Game.Role ro = Enum.Parse<Game.Role>(match.Groups[2].Value.Replace(" ", "_"));
                        Console.WriteLine($"Player {match.Groups[1].Value} is {ro}");
                        Player p = new Player(match.Groups[1].Value, ro, Game.Faction.Unknown);
                        Match m2 = Regex.Match(next, "<span><div class=\"tooltipprev\"> ⎗ <span class=\"tooltiptext\"><span style=\"color:#[A-Z0-9]+\"> ([A-Z0-9]+) </span></span></span></div>".ToUpper().Replace("%DIGIT%", "\\d+").Replace("%LINEBREAK%", "\\r\\n"));

                        if (m2.Success)
                        {
                            Console.WriteLine($"Detected a transformed role, turning the role into {m2.Groups[1].Value}.");
                            p.OgRole = Enum.Parse<Game.Role>(m2.Groups[1].Value.Replace(" ", "").ToUpper());
                            index++;
                        }
                        game.playersAlive.Add(p);

                        index++;
                        me++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message} in {ex.StackTrace}");
                        Console.WriteLine($"Failed to parse the role {match.Groups[2].Value}, skipping game...");
                        con = true;
                        skipped += $"{fileName}, ";
                        break;
                    }

                }

                if (con) continue;
                Console.WriteLine($"Total Roles detected: {me}");
                List<Match> matches = html.GetMatches("<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\">(?:<sprite=\"[A-Z]+\" name=\"PlayerNumbers_%digit%\">)?(?:<color=#[A-Z0-9]+>)?)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>".Replace("%DIGIT%", "\\d+"));
                foreach (Match match in matches)
                {
                    Console.WriteLine("Detected Death of: " + match.Groups[1].Value);
                    game.playersAlive[game.playersAlive.FindIndex(x => x.Name == match.Groups[1].Value)].status = Player.Status.Dead;
                }

                List<Match> jestWins = html.GetMatches("<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r45\">)?(?:<sprite=\"RoleIcons\" name=\"Role45\">)?<b><color=#[A-Z0-9]+>Jester</color></b></link></font></style> will get their revenge from the grave!</span>");
                List<Match> exeWins = html.GetMatches("<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r44\">(?:<sprite=\"RoleIcons\" name=\"Role44\">)?<b><color=#[A-Z0-9]+>Executioner</color></b></link></font></style> and left town.</span>".Replace("%DIGIT%", "\\d+"));

                foreach (Match m in exeWins)
                {
                    Console.WriteLine("Found winner Exe");
                    int n = game.playersAlive.FindIndex(x => x.Name == m.Groups[1].Value);
                    game.playersAlive[n].status = Player.Status.Left_Town;
                }
                List<Match> doomWins = html.GetMatches("<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r43\">(?:<sprite=\"RoleIcons\" name=\"Role43\">)?<b><color=#[A-Z0-9]+>Doomsayer</color></b></link></font></style> and left town.</span>".Replace("%DIGIT%", "\\d+"));
                foreach (Match m in doomWins)

                {
                    Console.WriteLine("Found winner Doomsayer");
                    Console.WriteLine(m.Value);

                    int n = game.playersAlive.FindIndex(x => x.Name == m.Groups[1].Value);



                    game.playersAlive[n].status = Player.Status.Left_Town;
                }
                List<Match> pirateWins = html.GetMatches("<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_%digit%\"><color=#[A-Z0-9]+>)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r46\">(?:<sprite=\"RoleIcons\" name=\"Role46\">)?<b><color=#[A-Z0-9]+>Pirate</color></b></link></font></style> and left town.</span>".Replace("%DIGIT%", "\\d+"));
                foreach (Match m in pirateWins)
                {
                    Console.WriteLine("Found winner Pirate");
                    int n = game.playersAlive.FindIndex(x => x.Name == m.Groups[1].Value);
                    Console.WriteLine(game.playersAlive[n].Name);
                    Console.WriteLine(n);
                    game.playersAlive[n].status = Player.Status.Left_Town;
                }
                string aliveRoles = "None";
                if (game.playersAlive.Count(x => x.status == Player.Status.Alive) != 0)
                {
                    aliveRoles = "";
                    foreach (Game.Role role in game.GetRolesAlive())
                    {
                        aliveRoles += role.ToString() + " | ";
                    }
                    aliveRoles = aliveRoles.Remove(aliveRoles.Length - 3);
                }
                Console.WriteLine($"Roles alive at the end of the game: {aliveRoles}");
                string winnedBy = "Killing opposing factions";
                Game.Faction factWin = game.playersAlive.GetWinningFaction();
                var rolesAlive = game.GetRolesAlive();
                if (factWin == Game.Faction.Draw)
                {
                    if (html.Match("<span style=\"background-color:#FF5500; color:#FFFFFF\"><br>(?:[A-Z0-9 ,.!-<=/>\"]+)?<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r253\">(?:<sprite=\"RoleIcons\" name=\"Role253\">)?<b><color=#[A-Z0-9]+>Death</color></b></link></font></style>(?:[A-Z0-9 ,.!-<=/>\"]+)?</span>").Success && game.playersAlive.GetAliveRoles().Contains(Game.Role.SOUL_COLLECTOR)) { factWin = Game.Faction.Apocalypse; winnedBy = "Death Transformation"; }
                    else if (rolesAlive.Contains(Game.Role.HEX_MASTER)) { factWin = Game.Faction.Coven; winnedBy = "Hex-Bomb"; }
                    else if (rolesAlive.Contains(Game.Role.SERIAL_KILLER) && game.GetRolesAlive().Contains(Game.Role.SHROUD) && game.playersAlive.Count == 2) { factWin = Game.Faction.Draw; winnedBy = "None"; }
                    else if (rolesAlive.Count(x => ((int)x < (int)Game.Role.END_OF_NKs)) > 0 && rolesAlive.Count < 8) { factWin = Game.Faction.Unknown; gamelogs += $"{fileName}, "; winnedBy = "None"; }
                    else if (rolesAlive.Count > 7) { factWin = Game.Faction.Corrupted; corrup += $"{fileName}, "; winnedBy = "None"; }
                    else winnedBy = "None";
                }
                Console.WriteLine($"Faction who won: {factWin} via {winnedBy}");
                List<Match> dcs = html.GetMatches("<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"%digit%\">(?:<sprite=\"[A-Z]+\" name=\"PlayerNumbers_%digit%\">)?(?:<color=#[A-Z0-9]+>)?)?(?:<b>)?([A-Z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>%linebreak%?<span style=\"background-color:#(FF0000|000000); color:#FFFFFF\"><br>(.+)?They (also )?disconnected from life\\.</span>".Replace("%LINEBREAK%", "\\r\\n").Replace("%DIGIT%", "\\d+"));
                string nDcs = "";
                Console.WriteLine($"{dcs.Count} dc(s) detected.");
                if (dcs.Count != 0)
                {
                    foreach (Match match1 in dcs)
                    {
                        nDcs += $"{game.Roles[match1.Groups[1].Value]} | ";
                    }
                    nDcs = nDcs.Remove(nDcs.Length - 3);
                    Console.WriteLine($"Roles that dcd: {nDcs}");
                }
                else
                {
                    nDcs = "None";
                }
                var w = Regex.Matches(htmlFullText, "<span style=\"background-color:#[A-Z0-9]+; color:#[A-Z0-9]+\"><br><br>Day (%digit%)</span>".ToUpper().Replace("%DIGIT%", "\\d+"));

                string textToAdd = $"\n{fileName}, {(factWin == Game.Faction.Corrupted || w.Count == 0 ? "Unknown" : w.Last().Groups[1].Value)}"; //TODO
                for (int i = 0; i < (int)Game.Role.END_OF_NEs; i++)
                {
                    Game.Role role = (Game.Role)i;
                    if (role.GetFaction() == Game.Faction.None) continue;
                    if (!btos && (((int)role > (int)Game.Role.END_OF_NEs) || role == Game.Role.BANSHEE || role == Game.Role.INQUISITOR)) continue;
                    textToAdd += $", {game.playersAlive.GetNumberOf(role)}";
                }
                textToAdd += $", {doomWins.Count}, {exeWins.Count}, {jestWins.Count}, {pirateWins.Count}, {dcs.Count}, {nDcs}, {aliveRoles}, {factWin.ToString().Replace("_", " ")}, {winnedBy}";
                for (int i = 1; i <= 15; i++)
                {

                }
                File.AppendAllText(csv, textToAdd);
                //break;
            }
            if (gamelogs.Length > 0)
            {
                Console.WriteLine($"These are gamelogs that i cannot know who won, you should go and manually check if they were (marked as \"Unknown\" in output.csv), they are usually auto-wins:");
                Console.WriteLine(gamelogs.Remove(gamelogs.Length - 2));
            }
            if (corrup.Length > 0)
            {
                Console.WriteLine("Corrupted Gamelogs: ");
                Console.WriteLine(corrup.Remove(corrup.Length - 2));
            }
            if (skipped.Length > 0)
            {
                Console.WriteLine("Skipped Gamelogs by mismatched roles: ");
                Console.WriteLine(skipped.Remove(skipped.Length - 2));
            }
            Console.WriteLine($"output.csv saved in {csv}");
            Console.WriteLine($"Press any key to close the program.");
            Console.ReadKey();
        }
    }
    public class Game
    {
        public enum Role
        {
            NONE,
            ADMIRER,
            AMNESIAC,
            BODYGUARD,
            CLERIC,
            CORONER,
            CRUSADER,
            DEPUTY,
            INVESTIGATOR,
            JAILOR,
            LOOKOUT,
            MARSHAL,
            MAYOR,
            MONARCH,
            PROSECUTOR,
            PSYCHIC,
            RETRIBUTIONIST,
            SEER,
            SHERIFF,
            SOCIALITE,
            SPY,
            TAVERN_KEEPER,
            TRACKER,
            TRAPPER,
            TRICKSTER,
            VETERAN,
            VIGILANTE,
            END_OF_TOWN,
            BANSHEE,
            CONJURER,
            COVEN_LEADER,
            DREAMWEAVER,
            ENCHANTER,
            HEX_MASTER,
            ILLUSIONIST,
            JINX,
            MEDUSA,
            NECROMANCER,
            POISONER,
            POTION_MASTER,
            RITUALIST,
            VOODOO_MASTER,
            WILDLING,
            WITCH,
            END_OF_COVEN,
            BAKER,
            BERSERKER,
            PLAGUEBEARER,
            SOUL_COLLECTOR,
            END_OF_APOCs,
            ARSONIST,
            SERIAL_KILLER,
            SHROUD,
            WEREWOLF,
            END_OF_NKs,
            DOOMSAYER,
            EXECUTIONER,
            INQUISITOR,
            JESTER,
            PIRATE,
            END_OF_NEs,
            AUDITOR,
            JUDGE,
            STARSPAWN,
            END_OF_NPs,
            CURSED_SOUL,
            JACKAL,
            VAMPIRE,
            END_OF_NSs,

            FAMINE = 140,
            WAR = 141,
            DEATH = 142,
            PESTILENCE = 143
        };
        public enum Faction
        {
            Draw,
            Town,
            Coven,
            Serial_Killers,
            Arsonists,
            Shrouds,
            Werewolves,
            Apocalypse,
            Jackal,
            Cursed_Souls,
            Vampires,
            Neutral_Evils,
            Unknown,
            Corrupted,
            None = 10
        };
        public Dictionary<string, Role> Roles = new();
        public List<Player> playersAlive = new();
        public List<Role> GetRolesAlive() => playersAlive.Where(x => x.status == Player.Status.Alive).Select(x => x.Role).ToList();
    }
    public class Player
    {
        public string Name;
        public Game.Role Role;
        public Game.Role OgRole;
        public Game.Faction Faction;
        public enum Status
        {
            Alive,
            Dead,
            Left_Town
        };
        public Status status;
        public string cause = "";
        public int whenDied = 0;
        public Player(string name, Game.Role role, Game.Faction faction, Game.Role ogRole = Game.Role.NONE)
        {
            Name = name;
            Role = role;
            OgRole = ogRole == Game.Role.NONE ? role : ogRole;
            Faction = faction;
            status = Status.Alive;
        }
    }
    static class Extensions
    {
        public static Game.Role GetHorsemen(Game.Role role, bool inverted = false)
        {
            int r = (int)Game.Role.FAMINE - (int)Game.Role.BAKER;
            return (Game.Role)((int)role + (r * (inverted ? -1 : 1)));
        }
        public static Game.Faction GetFaction(this Game.Role role)
        {
            int rint = (int)role;
            if (role.ToString().StartsWith("END") || role == Game.Role.NONE) return Game.Faction.None;
            if (rint > (int)Game.Role.END_OF_NKs && rint < (int)Game.Role.END_OF_NEs) return Game.Faction.Neutral_Evils;
            if (rint < (int)Game.Role.END_OF_TOWN) return Game.Faction.Town;
            if (rint < (int)Game.Role.END_OF_COVEN) return Game.Faction.Coven;
            if (rint < (int)Game.Role.END_OF_APOCs || rint > (int)Game.Role.END_OF_NSs) return Game.Faction.Apocalypse;
            if (role == Game.Role.SERIAL_KILLER) return Game.Faction.Serial_Killers;
            if (role == Game.Role.SHROUD) return Game.Faction.Shrouds;
            if (role == Game.Role.ARSONIST) return Game.Faction.Arsonists;
            if (role == Game.Role.WEREWOLF) return Game.Faction.Werewolves;
            if (role == Game.Role.VAMPIRE) return Game.Faction.Vampires;
            if (role == Game.Role.JACKAL) return Game.Faction.Jackal;
            if (role == Game.Role.CURSED_SOUL) return Game.Faction.Cursed_Souls;
            return Game.Faction.None;
        }
        public static Game.Faction GetWinningFaction(this List<Player> list)
        {
            List<Game.Faction> factionsAlive = new();
            foreach (Player p in list)
            {
                if (p.status != Player.Status.Alive) continue;
                var role = p.Role;

                int rint = (int)role;
                if (rint > (int)Game.Role.END_OF_NKs && rint < (int)Game.Role.END_OF_NEs) continue;
                if (rint < (int)Game.Role.END_OF_TOWN) { if (!factionsAlive.Contains(Game.Faction.Town)) factionsAlive.Add(Game.Faction.Town); continue; }
                if (rint < (int)Game.Role.END_OF_COVEN) { if (!factionsAlive.Contains(Game.Faction.Coven)) factionsAlive.Add(Game.Faction.Coven); continue; }
                if (rint < (int)Game.Role.END_OF_APOCs || rint > (int)Game.Role.END_OF_NSs) { if (!factionsAlive.Contains(Game.Faction.Apocalypse)) factionsAlive.Add(Game.Faction.Apocalypse); continue; }
                if (role == Game.Role.SERIAL_KILLER) { if (!factionsAlive.Contains(Game.Faction.Serial_Killers)) factionsAlive.Add(Game.Faction.Serial_Killers); continue; }
                if (role == Game.Role.SHROUD) { if (!factionsAlive.Contains(Game.Faction.Shrouds)) factionsAlive.Add(Game.Faction.Shrouds); continue; }
                if (role == Game.Role.ARSONIST) { if (!factionsAlive.Contains(Game.Faction.Arsonists)) factionsAlive.Add(Game.Faction.Arsonists); continue; }
                if (role == Game.Role.WEREWOLF) { if (!factionsAlive.Contains(Game.Faction.Werewolves)) factionsAlive.Add(Game.Faction.Werewolves); continue; }
                if (role == Game.Role.VAMPIRE) { if (!factionsAlive.Contains(Game.Faction.Vampires)) factionsAlive.Add(Game.Faction.Vampires); continue; }
                if (role == Game.Role.JACKAL) { if (!factionsAlive.Contains(Game.Faction.Jackal)) factionsAlive.Add(Game.Faction.Jackal); continue; }
                if (role == Game.Role.CURSED_SOUL) { if (!factionsAlive.Contains(Game.Faction.Cursed_Souls)) factionsAlive.Add(Game.Faction.Cursed_Souls); continue; }
            }
            Console.WriteLine("Found factions:" + string.Join(", ", factionsAlive));
            if (factionsAlive.Count == 1) return factionsAlive[0];
            return Game.Faction.Draw;
        }
        public static int GetNumberOf(this List<Player> list, Game.Role role)
        {
            return list.Count(item => item.OgRole == role);
        }
        public static List<Game.Role> GetAliveRoles(this List<Player> list)
        {
            return list.Where(x => x.status == Player.Status.Alive).Select(x => x.OgRole).ToList();
        }
        public static List<Match> GetMatches(this List<string> list, string regex)
        {
            List<Match> matches = new();
            regex = regex.ToUpper().Replace("%DIGIT%", "\\d+");
            foreach (string line in list)
            {
                Match match = Regex.Match(line, regex);
                if (match.Success) matches.Add(match);
            }
            return matches;
        }
        public static bool IsExactMatch(this string s, string regex)
        {
            Match match = Regex.Match(s, regex);
            if (!match.Success) return false;
            return match.Value.Length == s.Length - 1 || match.Value.Length == s.Length;
        }
        public static Match Match(this List<string> list, string regex)
        {
            regex = regex.ToUpper().Replace("%DIGIT%", "\\d+");
            foreach (string line in list)
            {
                Match match = Regex.Match(line, regex);
                if (match.Success) return match;
            }
            return Regex.Match(list[0], regex);
        }
    }
}
