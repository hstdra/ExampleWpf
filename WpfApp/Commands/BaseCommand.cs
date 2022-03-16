using System;
using System.Windows.Input;
using CommonServiceLocator;
using MediatR;

namespace WpfApp.Commands
{
    public abstract class BaseCommand : ICommand, IRequest
    {
        #pragma warning disable 67
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        #pragma warning restore 67

        bool ICommand.CanExecute(object? parameter)
        {
            return true;
        }

        void ICommand.Execute(object? parameter)
        {
            ServiceLocator.Current.GetInstance<IMediator>().Send(this);
        }
    }
}