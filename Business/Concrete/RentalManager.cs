using System.Collections.Generic;
using System.Linq;
using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Concrete
{
    public class RentalManager : IRentalService
    {
        private readonly ICarService _carService;
        private readonly IFindeksService _findeksService;
        private readonly IRentalDal _rentalDal;

        public RentalManager(IRentalDal rentalDal, ICarService carService, IFindeksService findeksService)
        {
            _carService = carService;
            _findeksService = findeksService;
            _rentalDal = rentalDal;
        }

        [SecuredOperation("user,rental.get,admin")]
        public IDataResult<Rental> GetById(int id)
        {
            return new SuccessDataResult<Rental>(_rentalDal.Get(r => r.Id == id));
        }

        [SecuredOperation("user,rental.get,admin")]
        public IDataResult<List<Rental>> GetAll()
        {
            return new SuccessDataResult<List<Rental>>(_rentalDal.GetAll());
        }

        public IDataResult<List<Rental>> GetAllByCarId(int carId)
        {
            return new SuccessDataResult<List<Rental>>(_rentalDal.GetAll(r => r.CarId == carId));
        }

        [SecuredOperation("user,rental.add,admin")]
        [ValidationAspect(typeof(RentalValidator))]
        public IResult Add(Rental rental)
        {
            var result = BusinessRules.Run(IsRent(rental), CheckFindexScoreSufficiency(rental));
            if (result != null) 
                return result;

            _rentalDal.Add(rental);

            return new SuccessResult(Messages.RentalAdded);
        }

        [SecuredOperation("rental.update,admin")]
        [ValidationAspect(typeof(RentalValidator))]
        public IResult Update(Rental customer)
        {
            _rentalDal.Update(customer);

            return new SuccessResult(Messages.RentalUpdated);
        }

        [SecuredOperation("rental.delete,admin")]
        public IResult Delete(Rental customer)
        {
            _rentalDal.Delete(customer);

            return new SuccessResult(Messages.RentalDeleted);
        }

        public IResult CheckReturnDateByCarId(int carId)
        {
            var result = _rentalDal.GetAll(x => x.CarId == carId && x.ReturnDate == null);
            if (result.Count > 0) 
                return new ErrorResult();

            return new SuccessResult();
        }

        [ValidationAspect(typeof(RentalValidator))]
        public IResult IsRent(Rental rental)
        {
            var result = _rentalDal.GetAll(r => r.CarId == rental.CarId);

            if (result.Any(r => r.RentEndDate >= rental.RentStartDate && r.RentStartDate <= rental.RentEndDate)) 
                return new ErrorResult();

            return new SuccessResult();
        }

        public IResult CheckFindexScoreSufficiency(Rental rental)
        {
            var car = _carService.GetById(rental.CarId).Data;
            var findeks = _findeksService.GetByCustomerId(rental.CustomerId).Data;

            if (findeks == null)
                return new ErrorResult();

            if (findeks.Score < car.MinFindeksScore) 
                return new ErrorResult();

            return new SuccessResult();
        }
    }
}