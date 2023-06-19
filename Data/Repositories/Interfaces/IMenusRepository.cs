using System;
using Plants.info.API.Data.Models;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Services
{
	public interface IMenusRepository
	{
		Task<IEnumerable<Genus>> getGenusList();
		Task<Genus?> getGenusById(int genusId); 

    }
}

