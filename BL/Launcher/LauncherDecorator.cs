namespace UNWcfTester.BL.Launcher
{
   public class LauncherDecorator : Launcher
   {
      private Launcher _launcher;

      public void SetLauncher(Launcher launcher)
      {
         this._launcher = launcher;
      }

      public override object Invoke()
      {
         return _launcher.Invoke();
      }
   }
}
