using AutoMapper;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Mapper;

public interface IMapFrom<out TSource>
{
    public void Mapping(Profile profile) => profile.CreateMap(typeof(TSource), GetType());
}
