import ContactForm from "./contact-form";
import ContactInfo from "./contact-info";
import CurveSvg from "./curve-svg";
import ContactMap from "./contact-map";

function ContactUsPage() {
  return (
    <div className="flex h-screen overflow-hidden relative p-0 m-0">
      {/* Left Panel */}
      <div className="relative w-1/3 bg-[#432B1F] text-white z-10">
        <div className="pl-35 flex flex-col justify-center h-full">
          <ContactInfo />
        </div>
        <CurveSvg />
      </div>

      {/* Right Panel */}
      <div className="w-2/3 bg-[#F5F0EA] relative z-10 flex items-center h-full">
        <div className="flex flex-row items-center justify-between w-full max-w-[1300px] mx-auto">
        <ContactForm />
        <ContactMap />
        </div>
      </div>
    </div>
  );
}

export default ContactUsPage;
