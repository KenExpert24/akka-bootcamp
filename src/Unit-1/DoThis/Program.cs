using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            // make tailCoordinatorActor
            var tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            var tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            // Never use this was of instansiating an actor in real world applications!!
            //var consoleWriterProps = Props.Create(typeof(ConsoleWriterActor));
            var consoleWriterProps = Props.Create(() => new ConsoleWriterActor());
            // Can/Should the name include an ID of that object?
            var consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            // This way of instansiating is preferred. // replaced with fileValidatorActor
            //var validationProps = Props.Create(() => new ValidationActor(consoleWriterActor));
            //var validationActor = MyActorSystem.ActorOf(validationProps, "validationActor");

            // pass tailCoordinatorActor to fileValidatorActorProps (just adding one extra arg)
            var fileValidatorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            var fileValidatorActor = MyActorSystem.ActorOf(fileValidatorProps, "validationActor");

            // This is fine, too.
            var consoleReaderProps = Props.Create<ConsoleReaderActor>(fileValidatorActor);
            var consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // tell console reader to begin
            //YOU NEED TO FILL IN HERE
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
