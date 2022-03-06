using System;
using System.Windows.Input;
using CommonServiceLocator;
using MediatR;

namespace WpfApp.Commands
{
    public abstract class BaseCommand : ICommand, IRequest
    {
        public event EventHandler? CanExecuteChanged;

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