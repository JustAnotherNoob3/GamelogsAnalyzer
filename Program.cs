using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using System.Net;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
namespace Html_Analizer
{
    class Program
    {
        static string[] allowedStrings = [
            "<span><br>\\[\\d+\\] ([A-Za-z0-9 ]+) - </span>",
            "<span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z ]+)(?=(?: \\[\\d+\\])? </span>)",
            "<span><br>\\[\\d+\\] [A-Za-z0-9 ]+ - </span><span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z ]+)(?: \\[\\d+\\])? </span>",
            "<span><div class=\"tooltipprev\"> ⎗ <span class=\"tooltiptext\"><span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z0-9]+) </span></span></span></div>",
            "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\">(?:<sprite=\"[A-Za-z]+\" name=\"PlayerNumbers_\\d+\">)?(?:<color=#[A-Za-z0-9]+>)?)?(?:<b>)?([A-Za-z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>",
            "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r45\">)?(?:<sprite=\"RoleIcons\" name=\"Role45\">)?<b><color=#[A-Za-z0-9]+>Jester</color></b></link></font></style> will get their revenge from the grave!</span>",
            "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r44\">)?(?:<sprite=\"RoleIcons\" name=\"Role44\">)?(?:<b>)?<color=#[A-Za-z0-9]+>Executioner</color></b></link></font></style> (has )?successfully got(ten)? their target hanged!</span>",
            "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_\\d+\"><color=#[A-Za-z0-9]+>)?(?:<b>)?[A-Za-z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r43\">(?:<sprite=\"RoleIcons\" name=\"Role43\">)?<b><color=#[A-Za-z0-9]+>Doomsayer</color></b></link></font></style> and left town.</span>",
            "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_\\d+\"><color=#[A-Za-z0-9]+>)?(?:<b>)?[A-Za-z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r46\">(?:<sprite=\"RoleIcons\" name=\"Role46\">)?<b><color=#[A-Za-z0-9]+>Pirate</color></b></link></font></style> and left town.</span>",
            "<span style=\"background-color:#FF5500; color:#FFFFFF\"><br>(?:[A-Za-z0-9 ,.!-<=/>\"]+)?<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r253\">(?:<sprite=\"RoleIcons\" name=\"Role253\">)?<b><color=#[A-Za-z0-9]+>Death</color></b></link></font></style>(?:[A-Za-z0-9 ,.!-<=/>\"]+)?</span>",
            "<span style=\"background-color:#(FF0000|000000); color:#FFFFFF\"><br>(.+)?They (also )?disconnected from life\\.</span>",
            ];
        static void Main(string[] args)
        {
            Console.WriteLine("Gamelogs analyzer.");
            string name = "";
            string rootdir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location!)!;
            string dir = "";
        initialize:
            Console.WriteLine("please write the name of the folder that contains all html. (Must be on the same folder as this program).");
            name = Console.ReadLine()!;
            dir = Path.Combine(rootdir, name);
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"path '{dir}' was not found, maybe you misspelled the name?");
                goto initialize;
            }
            string csv = Path.Combine(rootdir, "output.csv");
            if (!File.Exists(csv))
            {
                Console.WriteLine($"Creating output.csv");
                File.Create(csv).Close();
                File.WriteAllText(csv, "Date, # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won");
            }
            else
            {
                Console.WriteLine($"output.csv already exists, do you want to erase all the data within?(you can create a copy if you want) type 'y' if yes.");
                if (Console.ReadLine() == "y")File.WriteAllText(csv, "Date, # of admirers, # of amnes, # of BGs, # of clerics, # of coroners, # of crusaders, # of deputies, # of invests, Had jailor, # of LOs, Had mayor, Had monarch, Had prosecutor, # of psychics, # of retris, # of seers, # of sheriffs, # of spies, # of tavern keepers, # of trackers, # of trappers, # of tricksters, # of vets, # of vigis, Had conjurer, Had CL, Had dreamweaver, Had enchanter, Had HM, Had illu, Had jinx, Had dusa, Had necro, Had poisoner, Had PM, Had rit, Had VM, Had wildling, Had witch, Had baker, Had bers, Had PB, Had SC, # of doomsayers, # of exes, # of jesters, Had pirate, # of arsos, # of sks, # of shrouds, # of WWs, # of doomsayer wins, # of exe wins, # of jester wins, Pirate won, # of dcs, Roles of dcs, Roles alive at the end of the match, Faction win, How they won");

            }
            string gamelogs = "";
            string[] files = Directory.GetFiles(dir);
            string corrup = "";
            foreach (string file in files)
            {
                Game game = new();
                string fileName = Path.GetFileNameWithoutExtension(file);
                if(string.IsNullOrWhiteSpace(fileName)){
                    Console.WriteLine($"Skipping file with no name...");
                    continue;
                }
                Console.WriteLine($"Starting to parse {fileName}");
                string htmlFullText = File.ReadAllText(file);
                string html = "";
                foreach(string p in htmlFullText.Split("\n")){
                    bool add = false;
                    foreach(string tryMatch in allowedStrings){
                        if(!Regex.IsMatch(p, tryMatch)) continue;
                        add = true;
                        break;
                    }
                    if(!add) continue;
                    html += p+"\n";
                }
                try{html.Remove(html.Length-3);}
                catch{
                    Console.WriteLine($"File: {fileName} has no content.");
                    continue;
                }
                Console.WriteLine(html);
                MatchCollection matches = Regex.Matches(html, "<span><br>\\[\\d+\\] ([A-Za-z0-9 ]+) - </span>(?:[\\r\\n]+)?<span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z ]+)(?=(?: \\[\\d+\\])? </span>)");
                bool con = false;
                int me = 0;
                Dictionary<string, Game.Role> actualRoles = new();
                foreach (Match match in matches)
                {
                    try
                    {
                        Game.Role ro = Enum.Parse<Game.Role>(match.Groups[2].Value.Replace(" ", "").ToUpper());
                        game.Roles.Add(match.Groups[1].Value, ro);
                        actualRoles.Add(match.Groups[1].Value, ro);
                        game.playersAlive.Add(ro);
                        Console.WriteLine($"Detected role: {ro}");
                        me++;
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to parse the role {match.Groups[2].Value}, skipping game...");
                        con = true;
                        break;
                    }
                }
                if (con) continue;
                Console.WriteLine($"Total Roles detected: {me}");

                matches = Regex.Matches(html, "<span><br>\\[\\d+\\] [A-Za-z0-9 ]+ - </span><span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z ]+)(?: \\[\\d+\\])? </span>[\\r\\n]+<span><div class=\"tooltipprev\"> ⎗ <span class=\"tooltiptext\"><span style=\"color:#[A-Za-z0-9]+\"> ([A-Za-z0-9]+) </span></span></span></div>");
                foreach (Match match in matches)
                {
                    Console.WriteLine($"Detected a transformed role, turning first {match.Groups[1].Value} into {match.Groups[2].Value}");
                    game.Roles[game.Roles.FirstOrDefault(x => x.Value == Enum.Parse<Game.Role>(match.Groups[1].Value.Replace(" ", "").ToUpper())).Key] = Enum.Parse<Game.Role>(match.Groups[2].Value.Replace(" ", "").ToUpper());
                }
                matches = Regex.Matches(html, "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\">(?:<sprite=\"[A-Za-z]+\" name=\"PlayerNumbers_\\d+\">)?(?:<color=#[A-Za-z0-9]+>)?)?(?:<b>)?([A-Za-z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>");
                foreach (Match match in matches)
                {
                    Game.Role l = actualRoles[match.Groups[1].Value];
                    if ((int)l > 100) l = (Game.Role)((int)l - 100);
                    game.playersAlive.Remove(l);
                }
                MatchCollection jestWins = Regex.Matches(html, "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r45\">)?(?:<sprite=\"RoleIcons\" name=\"Role45\">)?<b><color=#[A-Za-z0-9]+>Jester</color></b></link></font></style> will get their revenge from the grave!</span>");
                MatchCollection exeWins = Regex.Matches(html, "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>The <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?>(?:<link=\"r44\">)?(?:<sprite=\"RoleIcons\" name=\"Role44\">)?(?:<b>)?<color=#[A-Za-z0-9]+>Executioner</color></b></link></font></style> (has )?successfully got(ten)? their target hanged!</span>");
                for (int i = 0; i < exeWins.Count; i++)
                {
                    Console.WriteLine("Removing winner Exe");
                    bool a = game.playersAlive.Remove(Game.Role.EXECUTIONER);
                    Console.WriteLine(a);
                }
                MatchCollection doomWins = Regex.Matches(html, "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_\\d+\"><color=#[A-Za-z0-9]+>)?(?:<b>)?[A-Za-z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r43\">(?:<sprite=\"RoleIcons\" name=\"Role43\">)?<b><color=#[A-Za-z0-9]+>Doomsayer</color></b></link></font></style> and left town.</span>");
                for (int i = 0; i < doomWins.Count; i++)
                {
                    Console.WriteLine("Removing winner Doomsayer");
                    bool a = game.playersAlive.Remove(Game.Role.DOOMSAYER);
                    Console.WriteLine($"successful? {a}");
                }
                MatchCollection pirateWins = Regex.Matches(html, "<span style=\"background-color:#000000; color:#FF7F66\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\"><sprite=\"PlayerNumbers\" name=\"PlayerNumbers_\\d+\"><color=#[A-Za-z0-9]+>)?(?:<b>)?[A-Za-z0-9 ]+(?:</b>)?(</color></link></font></style>)? (?:has )?accomplished their goal as <style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r46\">(?:<sprite=\"RoleIcons\" name=\"Role46\">)?<b><color=#[A-Za-z0-9]+>Pirate</color></b></link></font></style> and left town.</span>");
                for (int i = 0; i < pirateWins.Count; i++)
                {
                    Console.WriteLine("Removing winner Pirate");
                    bool a = game.playersAlive.Remove(Game.Role.PIRATE);
                    Console.WriteLine($"successful? {a}");
                }
                string aliveRoles = "None";
                if (game.playersAlive.Count != 0)
                {
                    aliveRoles = "";
                    foreach (Game.Role role in game.playersAlive)
                    {
                        aliveRoles += role.ToString() + " | ";
                    }
                    aliveRoles = aliveRoles.Remove(aliveRoles.Length - 3);
                }
                Console.WriteLine($"Roles alive at the end of the game: {aliveRoles}");
                string winnedBy = "Killing opposing factions";
                Game.Faction factWin = game.playersAlive.GetWinningFaction();
                if (factWin == Game.Faction.Draw)
                {
                    if (Regex.IsMatch(html, "<span style=\"background-color:#FF5500; color:#FFFFFF\"><br>(?:[A-Za-z0-9 ,.!-<=/>\"]+)?<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"r253\">(?:<sprite=\"RoleIcons\" name=\"Role253\">)?<b><color=#[A-Za-z0-9]+>Death</color></b></link></font></style>(?:[A-Za-z0-9 ,.!-<=/>\"]+)?</span>") && game.playersAlive.Contains(Game.Role.SOULCOLLECTOR)) { factWin = Game.Faction.Apocalypse; winnedBy = "Death Transformation"; }
                    else if (game.playersAlive.Contains(Game.Role.HEXMASTER)) { factWin = Game.Faction.Coven; winnedBy = "Hex-Bomb"; }
                    else if (game.playersAlive.Contains(Game.Role.SERIALKILLER) && game.playersAlive.Contains(Game.Role.SHROUD) && game.playersAlive.Count == 2) { factWin = Game.Faction.Draw; winnedBy = "None"; }
                    else if (game.playersAlive.Count(x => ((int)x < 48)) > 0 && game.playersAlive.Count < 15) { factWin = Game.Faction.Unknown; gamelogs += $"{fileName}, "; winnedBy = "None"; }
                    else if (game.playersAlive.Count == 15) { factWin = Game.Faction.Corrupted; corrup += $"{fileName}, "; winnedBy = "None"; }
                    else winnedBy = "None";
                }
                Console.WriteLine($"Faction who won: {factWin} via {winnedBy}");
                MatchCollection dcs = Regex.Matches(html, "<span style=\"background-color:#(?:FF0000|000000); color:#FFFFFF\"><br>(?:<style=Mention(?:Mono)?><font=\"(?:Game SDF|NotoSans SDF)\"(?: material=\"(?:Game SDF|NotoSans SDF) - Mentions\")?><link=\"\\d+\">(?:<sprite=\"[A-Za-z]+\" name=\"PlayerNumbers_\\d+\">)?(?:<color=#[A-Za-z0-9]+>)?)?(?:<b>)?([A-Za-z0-9 ]+)(?:</b>)?(?:(?:</color>)?</link></font></style>)? died (?:last night|today)\\.(?:.+)?</span>[\\n\\r]+?<span style=\"background-color:#(FF0000|000000); color:#FFFFFF\"><br>(.+)?They (also )?disconnected from life\\.</span>");
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


                File.AppendAllText(csv, $"\n{fileName}, {game.Roles.GetNumberOf(Game.Role.ADMIRER)}, {game.Roles.GetNumberOf(Game.Role.AMNESIAC)}, {game.Roles.GetNumberOf(Game.Role.BODYGUARD)}, {game.Roles.GetNumberOf(Game.Role.CLERIC)}, {game.Roles.GetNumberOf(Game.Role.CORONER)}, {game.Roles.GetNumberOf(Game.Role.CRUSADER)}, {game.Roles.GetNumberOf(Game.Role.DEPUTY)}, {game.Roles.GetNumberOf(Game.Role.INVESTIGATOR)}, {game.Roles.GetNumberOf(Game.Role.JAILOR)}, {game.Roles.GetNumberOf(Game.Role.LOOKOUT)}, {game.Roles.GetNumberOf(Game.Role.MAYOR)}, {game.Roles.GetNumberOf(Game.Role.MONARCH)}, {game.Roles.GetNumberOf(Game.Role.PROSECUTOR)}, {game.Roles.GetNumberOf(Game.Role.PSYCHIC)}, {game.Roles.GetNumberOf(Game.Role.RETRIBUTIONIST)}, {game.Roles.GetNumberOf(Game.Role.SEER)}, {game.Roles.GetNumberOf(Game.Role.SHERIFF)}, {game.Roles.GetNumberOf(Game.Role.SPY)}, {game.Roles.GetNumberOf(Game.Role.TAVERNKEEPER)}, {game.Roles.GetNumberOf(Game.Role.TRACKER)}, {game.Roles.GetNumberOf(Game.Role.TRAPPER)}, {game.Roles.GetNumberOf(Game.Role.TRICKSTER)}, {game.Roles.GetNumberOf(Game.Role.VETERAN)}, {game.Roles.GetNumberOf(Game.Role.VIGILANTE)}, {game.Roles.GetNumberOf(Game.Role.CONJURER)}, {game.Roles.GetNumberOf(Game.Role.COVENLEADER)}, {game.Roles.GetNumberOf(Game.Role.DREAMWEAVER)}, {game.Roles.GetNumberOf(Game.Role.ENCHANTER)}, {game.Roles.GetNumberOf(Game.Role.HEXMASTER)}, {game.Roles.GetNumberOf(Game.Role.ILLUSIONIST)}, {game.Roles.GetNumberOf(Game.Role.JINX)}, {game.Roles.GetNumberOf(Game.Role.MEDUSA)}, {game.Roles.GetNumberOf(Game.Role.NECROMANCER)}, {game.Roles.GetNumberOf(Game.Role.POISONER)}, {game.Roles.GetNumberOf(Game.Role.POTIONMASTER)}, {game.Roles.GetNumberOf(Game.Role.RITUALIST)}, {game.Roles.GetNumberOf(Game.Role.VOODOOMASTER)}, {game.Roles.GetNumberOf(Game.Role.WILDLING)}, {game.Roles.GetNumberOf(Game.Role.WITCH)}, {game.Roles.GetNumberOf(Game.Role.BAKER)}, {game.Roles.GetNumberOf(Game.Role.BERSERKER)}, {game.Roles.GetNumberOf(Game.Role.PLAGUEBEARER)}, {game.Roles.GetNumberOf(Game.Role.SOULCOLLECTOR)}, {game.Roles.GetNumberOf(Game.Role.DOOMSAYER)}, {game.Roles.GetNumberOf(Game.Role.EXECUTIONER)}, {game.Roles.GetNumberOf(Game.Role.JESTER)}, {game.Roles.GetNumberOf(Game.Role.PIRATE)}, {game.Roles.GetNumberOf(Game.Role.ARSONIST)}, {game.Roles.GetNumberOf(Game.Role.SERIALKILLER)}, {game.Roles.GetNumberOf(Game.Role.SHROUD)}, {game.Roles.GetNumberOf(Game.Role.WEREWOLF)}, {doomWins.Count}, {exeWins.Count}, {jestWins.Count}, {pirateWins.Count}, {dcs.Count}, {nDcs}, {aliveRoles}, {factWin.ToString().Replace("_", " ")}, {winnedBy}");
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
            Console.WriteLine($"output.csv saved in {csv}");
            Console.WriteLine($"Press any key to close the program.");
            Console.ReadKey();
        }
    }
    class Game
    {
        public enum Role
        {
            NONE,
            ADMIRER,
            AMNESIAC,
            // Token: 0x04000A34 RID: 2612
            BODYGUARD,
            // Token: 0x04000A35 RID: 2613
            CLERIC,
            CORONER,
            // Token: 0x04000A37 RID: 2615
            CRUSADER,
            // Token: 0x04000A38 RID: 2616
            DEPUTY,
            // Token: 0x04000A39 RID: 2617
            INVESTIGATOR,
            // Token: 0x04000A3A RID: 2618
            JAILOR,
            // Token: 0x04000A3B RID: 2619
            LOOKOUT,
            // Token: 0x04000A3C RID: 2620
            MAYOR,
            // Token: 0x04000A3D RID: 2621
            MONARCH,
            // Token: 0x04000A3E RID: 2622
            PROSECUTOR,
            // Token: 0x04000A3F RID: 2623
            PSYCHIC,
            // Token: 0x04000A40 RID: 2624
            RETRIBUTIONIST,
            // Token: 0x04000A41 RID: 2625
            SEER,
            // Token: 0x04000A42 RID: 2626
            SHERIFF,
            // Token: 0x04000A43 RID: 2627
            SPY,
            // Token: 0x04000A44 RID: 2628
            TAVERNKEEPER,
            // Token: 0x04000A45 RID: 2629
            TRACKER,
            // Token: 0x04000A46 RID: 2630
            TRAPPER,
            // Token: 0x04000A47 RID: 2631
            TRICKSTER,
            // Token: 0x04000A48 RID: 2632
            VETERAN,
            // Token: 0x04000A49 RID: 2633
            VIGILANTE,
            // Token: 0x04000A4A RID: 2634
            CONJURER,
            // Token: 0x04000A4B RID: 2635
            COVENLEADER,
            // Token: 0x04000A4C RID: 2636
            DREAMWEAVER,
            // Token: 0x04000A4D RID: 2637
            ENCHANTER,
            // Token: 0x04000A4E RID: 2638
            HEXMASTER,
            // Token: 0x04000A4F RID: 2639
            ILLUSIONIST,
            // Token: 0x04000A50 RID: 2640
            JINX,
            // Token: 0x04000A51 RID: 2641
            MEDUSA,
            // Token: 0x04000A52 RID: 2642
            NECROMANCER,
            // Token: 0x04000A53 RID: 2643
            POISONER,
            // Token: 0x04000A54 RID: 2644
            POTIONMASTER,
            // Token: 0x04000A55 RID: 2645
            RITUALIST,
            // Token: 0x04000A56 RID: 2646
            VOODOOMASTER,
            // Token: 0x04000A57 RID: 2647
            WILDLING,
            // Token: 0x04000A58 RID: 2648
            WITCH,
            BAKER,
            BERSERKER,
            SOULCOLLECTOR,
            PLAGUEBEARER,
            SERIALKILLER,
            SHROUD,
            ARSONIST,
            WEREWOLF,
            DOOMSAYER,
            EXECUTIONER,
            JESTER,
            PIRATE,
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
            Unknown,
            Corrupted
        };
        public Dictionary<string, Role> Roles = new();
        public List<Role> playersAlive = new();
    }
    static class ListExtensions
    {

        public static Game.Faction GetWinningFaction(this List<Game.Role> list)
        {
            List<Game.Faction> factionsAlive = new();
            foreach (Game.Role role in list)
            {
                int rint = (int)role;
                if (rint > 47) continue;
                if (rint < 25) { if (!factionsAlive.Contains(Game.Faction.Town)) factionsAlive.Add(Game.Faction.Town); continue; }
                if (rint < 40) { if (!factionsAlive.Contains(Game.Faction.Coven)) factionsAlive.Add(Game.Faction.Coven); continue; }
                if (rint < 44) { if (!factionsAlive.Contains(Game.Faction.Apocalypse)) factionsAlive.Add(Game.Faction.Apocalypse); continue; }
                if (rint == 44) { if (!factionsAlive.Contains(Game.Faction.Serial_Killers)) factionsAlive.Add(Game.Faction.Serial_Killers); continue; }
                if (rint == 45) { if (!factionsAlive.Contains(Game.Faction.Shrouds)) factionsAlive.Add(Game.Faction.Shrouds); continue; }
                if (rint == 46) { if (!factionsAlive.Contains(Game.Faction.Arsonists)) factionsAlive.Add(Game.Faction.Arsonists); continue; }
                if (rint == 47) { if (!factionsAlive.Contains(Game.Faction.Werewolves)) factionsAlive.Add(Game.Faction.Werewolves); continue; }
            }
            if (factionsAlive.Count == 1) return factionsAlive[0];
            return Game.Faction.Draw;
        }
        public static int GetNumberOf(this Dictionary<string, Game.Role> list, Game.Role role)
        {
            return list.Count(item => item.Value == role);
        }
    }
}
