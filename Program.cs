using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PublisherExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var publisher = new Publisher();

            var notifierOne = new NotifierOne(publisher);
            var notifierTwo = new NotifierTwo(publisher);

            notifierOne.DoSomethingAndNotify();
            notifierTwo.DoSomethingAndNotify();

            Console.ReadLine();
        }
    }

    #region notifications
    
    class NotifierOne
    {
        private readonly Publisher _publisher;

        public NotifierOne(Publisher publisher)
        {
            _publisher = publisher;
        }

        public void DoSomethingAndNotify()
        {
            _publisher.Publish(new NotifyEventOne() { MetaDataOne = $"notification from {nameof(NotifierOne)}" });
        }
    }

    class NotifierTwo
    {
        private readonly Publisher _publisher;

        public NotifierTwo(Publisher publisher)
        {
            _publisher = publisher;
        }
        public void DoSomethingAndNotify()
        {
            _publisher.Publish(new NotifyEventTwo() { MetaDataTwo = $"notification from {nameof(NotifierTwo)}", User = "kenneth"});
        }
    }

    interface INotifyEvent
    {

    }

    class NotifyEventOne : INotifyEvent
    {
        public string MetaDataOne { get; set; }
    }

    class NotifyEventTwo : INotifyEvent
    {
        public string MetaDataTwo { get; set; }
        public string User { get; set; }
    }

    #endregion

    class Publisher
    {
        public void Publish(INotifyEvent notifyEvent)
        {
            var notifyEventType = notifyEvent.GetType();
            var subscriberWrapper = (SubscriberWrapper) Activator.CreateInstance(typeof(SubscriberWrapperImpl<>).MakeGenericType(notifyEventType));
            subscriberWrapper.Notify(notifyEvent);
        }
    }

    #region subscriptions

    interface ISubscriber<in T> where T : INotifyEvent
    {
        public void Notify(T notifyEvent);
    }

    abstract class SubscriberWrapper
    {
        public abstract void Notify(INotifyEvent notifyEvent);
    }

    class SubscriberWrapperImpl<T> : SubscriberWrapper where T : INotifyEvent
    {
        public override void Notify(INotifyEvent notifyEvent)
        {
            var openGenericType = typeof(ISubscriber<>);
            var subscriberTypes = from x in Assembly.GetAssembly(typeof(SubscriberWrapperImpl<>)).GetTypes()
                from z in x.GetInterfaces()
                let y = x.BaseType
                where
                    (y != null && y.IsGenericType &&
                     openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                    (z.IsGenericType &&
                     openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                    && z.GenericTypeArguments[0] == typeof(T)
                select x;

            foreach (var subscriberType in subscriberTypes)
            {
                var subscriberInstance = (ISubscriber<T>) Activator.CreateInstance(subscriberType);
                subscriberInstance.Notify((T)notifyEvent);
            }
        }
    }

    class SubscriberOneA : ISubscriber<NotifyEventOne>
    {
        public void Notify(NotifyEventOne notifyEvent)
        {
            Console.WriteLine($"{nameof(SubscriberOneA)} is notified with values :'{notifyEvent.MetaDataOne}'");
        }
    }

    class SubscriberOneB : ISubscriber<NotifyEventOne>
    {
        public void Notify(NotifyEventOne notifyEvent)
        {
            Console.WriteLine($"{nameof(SubscriberOneB)} is notified with values :'{notifyEvent.MetaDataOne}'");
        }
    }

    class SubscriberTwoA : ISubscriber<NotifyEventTwo>
    {
        public void Notify(NotifyEventTwo notifyEvent)
        {
            Console.WriteLine($"{nameof(SubscriberTwoA)} is notified with values :'{notifyEvent.MetaDataTwo}' and '{notifyEvent.User}'");
        }
    }

    class SubscriberTwoB : ISubscriber<NotifyEventTwo>
    {
        public void Notify(NotifyEventTwo notifyEvent)
        {
            Console.WriteLine($"{nameof(SubscriberTwoB)} is notified with values :'{notifyEvent.MetaDataTwo}' and '{notifyEvent.User}'");
        }
    }
    #endregion
}
