using System;
using System.Collections.Generic;
using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;

namespace Business.Concrete
{
    public class FindeksManager : IFindeksService
    {
        private readonly IFindeksDal _findeksDal;

        public FindeksManager(IFindeksDal findeksDal)
        {
            _findeksDal = findeksDal;
        }

        [SecuredOperation("findeks.get,admin")]
        public IDataResult<Findeks> GetById(int id)
        {
            return new SuccessDataResult<Findeks>(_findeksDal.Get(f => f.Id == id));
        }

        [SecuredOperation("user")]
        public IDataResult<Findeks> GetByCustomerId(int customerId)
        {
            var findeks = _findeksDal.Get(f => f.CustomerId == customerId);
            if (findeks == null) 
                return new ErrorDataResult<Findeks>();

            return new SuccessDataResult<Findeks>(findeks);
        }

        [SecuredOperation("findeks.get,admin")]
        public IDataResult<List<Findeks>> GetAll()
        {
            return new SuccessDataResult<List<Findeks>>(_findeksDal.GetAll());
        }

        [SecuredOperation("user")]
        public IResult Add(Findeks findeks)
        {
            var newFindeks = CalculateFindeksScore(findeks).Data;
            _findeksDal.Add(newFindeks);

            return new SuccessResult();
        }

        [SecuredOperation("findeks.update,admin")]
        public IResult Update(Findeks findeks)
        {
            var newFindeks = CalculateFindeksScore(findeks).Data;
            _findeksDal.Update(newFindeks);

            return new SuccessResult();
        }

        [SecuredOperation("findeks.delete,admin")]
        public IResult Delete(Findeks findeks)
        {
            _findeksDal.Delete(findeks);

            return new SuccessResult();
        }

        public IDataResult<Findeks> CalculateFindeksScore(Findeks findeks)
        {
            findeks.Score = (short) new Random().Next(0, 1901);

            return new SuccessDataResult<Findeks>(findeks);
        }
    }
}