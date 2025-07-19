using System.Threading.Tasks;
using Godot;

namespace desktoppet.scripts.utils;

public static class Utils
{
    
    public static async Task WaitFrames(Node contextNode, int count)
    {
        for(int i=0; i<count; i++)
        {
            await contextNode.ToSignal(contextNode.GetTree(), "process_frame");
            await contextNode.ToSignal(RenderingServer.Singleton, "frame_post_draw");
        }
    }
}