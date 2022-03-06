using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WpfApp.Services;

namespace WpfApp.Commands;

public class LoadProductCommand : BaseCommand
{
}

public class LoadProductCommandHandler : IRequestHandler<LoadProductCommand>
{
    private readonly IProductService _productService;

    public LoadProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<Unit> Handle(LoadProductCommand request, CancellationToken cancellationToken)
    {
        _productService.LoadProducts();

        return Unit.Task;
    }
}