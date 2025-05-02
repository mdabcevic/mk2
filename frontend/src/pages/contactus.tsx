import ContactInfo from "./contact-info";

function ContactUsPage() {
    return (
      <div className="flex h-screen">
        {/* Left Panel */}
        <div className="w-1/2 bg-[#432B1F] text-white p-10 flex flex-col justify-between">
          <ContactInfo />
        </div>
  
        {/* Right Panel (Placeholder for now) */}
        <div className="w-1/2 bg-[#F5F0EA] p-10">
          {/* We'll fill this in later with the form and map */}
          <div className="text-gray-800">Right Panel</div>
        </div>
      </div>
    );
  }
  
  export default ContactUsPage;
