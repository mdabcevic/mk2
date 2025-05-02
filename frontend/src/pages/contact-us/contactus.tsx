import ContactForm from "./contact-form";
import ContactInfo from "./contact-info";
import CurveSvg from "./curve-svg";
import ContactMap from "./contact-map";

function ContactUsPage() {
  return (
    <div className="flex flex-col lg:flex-row min-h-screen overflow-hidden">
      {/* Left Panel */}
<div className="relative w-full lg:w-2/5 bg-[#432B1F] text-white z-10">
  <div className="px-4 sm:px-6 lg:px-0 lg:pl-[8.75rem] py-10 flex flex-col justify-center h-full">

    <ContactInfo />
  </div>
  <div className="hidden lg:block">
  <CurveSvg />
</div>
</div>

      {/* Right Panel */}
      <div className="w-full lg:w-3/5 bg-[#F5F0EA] flex items-center justify-center py-10">
  <div className="flex flex-col lg:flex-row items-center justify-center w-full px-6 sm:px-10 lg:px-0">
    <ContactForm />
    <ContactMap />
  </div>
</div>
    </div>
  );
}

export default ContactUsPage;
