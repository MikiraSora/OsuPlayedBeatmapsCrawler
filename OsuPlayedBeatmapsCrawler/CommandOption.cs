using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler
{
    public class CommandOption
    {
        [Option("secert", Required = false, HelpText = "[option] program will load this file and get username and password()")]
        public string SecertFilePath { get; set; }

        [Option("username", Required = false, HelpText = "osu! user name")]
        public string UserName { get; set; }

        [Option("password", Required = false, HelpText = "osu! user password")]
        public string Password { get; set; }

        [Option("cursor", Required = false, Default = "", HelpText = "[option] cursor string")]
        public string Cursor { get; set; }

        [Option("save", Required = false, Default = "./beatmaps", HelpText = "[option] folder path which saves .osz beatmap files")]
        public string BeamtapSaveFolderPath { get; set; } = "./beatmaps";

        [Option("log", Required = false, Default = "./logs", HelpText = "[option] folder path which saves program log files")]
        public string LogFolderPath { get; set; } = "./logs";
    }
}
