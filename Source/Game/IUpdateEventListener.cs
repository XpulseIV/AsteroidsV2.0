namespace Asteroids2.Source.Game;

public interface IUpdateEventListener
{
    void OnUpdate(object sender, UpdateEventArgs e);
}