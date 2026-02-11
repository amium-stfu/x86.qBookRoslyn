//using Main;
//using static QB.Automation;
//using static qbSystem; //HALE //TODO get rid of this reference!!!

namespace QB.Automation
{
    /*
       public class Timer : QB.Timer
       {
           /// <summary>
           /// Creates a Timer-Object. 
           /// Use 'timer.start(interval, [repeatCount])' to start the timer
           /// Use 'timer.OnElapsed = () => {Action};' for actions
           /// </summary>
           public Timer(string text, int interval) :base(text, interval)
           {
               Resume();
           }

           public int RepeatCount = -1; //auto-restart

           public delegate void OnElapsedDelegate(QB.Timer sender);
           public OnElapsedDelegate OnElapsed;

           public override void Elapsed(QB.Timer sender)
           {
               base.Elapsed(sender);

               try
               {
                   if (OnElapsed != null)
                   {
                       OnElapsed(this);
                   }
               }
               catch (Exception ex)
               {
                   QB.Logger.Error("#EX in Timer.Elapsed: " + ex.Message);
               }

               if (RepeatCount >= 0)
               {
                   RepeatCount--;
                   if (RepeatCount == 0)
                   {
                       Stop();
                   }
               }
           }
       }
       */


    /*
    public class Counter : Module
    {
        public Counter(string text) : base(text)
        {
            if (Read.Value < Min)
                Read.Value = Min;
            if (Read.Value > Max)
                Read.Value = Max;
        }

        public Counter(string text, int interval, double min, double max, double step) : this(text)
        {
            Interval = interval;
            Min = min;
            Max = max;
            Step = step;
            Run();
        }

        public double Min = 0;
        public double Max = 100;
        public double Step = 1;

        public override void Elapsed(Timer s)
        {
            if (double.IsNaN(Read.Value))
                Read.Value = 0;

            double newValue = Read.Value;

            if (newValue < (Max - Step))
                newValue = Read.Value + Step;
            else
                newValue = Min;

            if (newValue < Min)
                newValue = Min;
            if (newValue > Max)
                newValue = Max;

            Read.Value = newValue;
        }
    
    }*/
}