using CommandSystem;
using EffectOrderer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectOrderer
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Debug: ICommand
    {
        public string Command => "edbg";
        public string[] Aliases => [];

        public string Description => "e debug";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plr = arguments.ElementAt(0);
            foreach (var eff in Player.Get(int.Parse(plr))!.ReferenceHub.playerEffectsController.AllEffects)
            {
                eff.GetManager().DebugPrints();
            }
            response = "";
            return true;
        }
    }
}
