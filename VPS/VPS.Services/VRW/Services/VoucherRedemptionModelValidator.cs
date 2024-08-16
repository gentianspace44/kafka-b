using FluentValidation;
using FluentValidation.Results;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Helpers;

namespace VPS.Services.VRW.Services
{
    public class VoucherRedemptionModelValidator : AbstractValidator<VrwViewModel>
    {


        public VoucherRedemptionModelValidator()
        {

        }
        public override ValidationResult Validate(ValidationContext<VrwViewModel> context)
        {
            RuleFor(v => v.VoucherNumber).Custom((voucherNumber, context) =>
            {
                VrwViewModel vrwModel = context.InstanceToValidate;

                if (vrwModel == null)
                {
                    return;
                }

                string voucherName = vrwModel.VoucherName;

                if (string.IsNullOrEmpty(voucherName))
                {
                    return;
                }

                var errorMsgLength = VoucherProviderHelper.IsVoucherLengthValid(voucherNumber, vrwModel.VoucherNumberLength);

                if (!string.IsNullOrWhiteSpace(errorMsgLength))
                {
                    context.AddFailure(errorMsgLength);
                }

            })
            .When(x => !x.DefaultVoucherProvider);
            RuleFor(v => v.DevicePlatform).NotEmpty().WithMessage("device required.");
            RuleFor(v => v.ClientId).NotEmpty().WithMessage("cid required.");
            RuleFor(v => v.VoucherNumber).NotNull().WithMessage("Please enter a voucher number.");
            RuleFor(v => v.VoucherName).NotNull().When(x => !x.DefaultVoucherProvider).WithMessage("Please select your voucher provider above.");

            return base.Validate(context);
        }
    }
}
