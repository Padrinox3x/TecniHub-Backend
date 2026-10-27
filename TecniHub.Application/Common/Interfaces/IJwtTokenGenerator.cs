using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecniHub.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(int usuarioId, string email, string rol);
}
