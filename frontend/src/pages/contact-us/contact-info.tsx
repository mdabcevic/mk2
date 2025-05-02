import { JSX } from "react";
import { FiMail, FiPhone, FiMapPin } from "react-icons/fi";
import SocialIcons from "./social-icons";

function ContactInfo() {
  return (
<div className="flex flex-col items-center lg:items-start text-center lg:text-left gap-12 w-full pt-12 lg:pt-0 lg:pl-[2.25rem] lg:gap-16">

      {/* Logo + Title */}
      <div>
        <h1 className="text-3xl sm:text-4xl font-bold">Contact Us</h1>
        <p className="text-base sm:text-lg mt-2">We’d love to hear from you!</p>
      </div>

      {/* Info Items */}
      <div className="flex flex-col lg:flex-col gap-4 lg:gap-6 items-center lg:items-start sm:flex-row sm:flex-wrap sm:justify-center lg:flex-col">

        <ContactItem icon={<FiMail />} label="info@mk.com" />
        <ContactItem icon={<FiPhone />} label="+385 123456789" />
        <ContactItem icon={<FiMapPin />} label="address 123" />
      </div>

      {/* Social Icons */}
      <SocialIcons className="mt-8 sm:mt-10 justify-center lg:justify-start" />
    </div>
  );
}

export default ContactInfo;

type ContactItemProps = {
  icon: JSX.Element;
  label: string;
};

function ContactItem({ icon, label }: ContactItemProps) {
  return (
    <div className="flex items-center gap-4">
      <div className="bg-[#F5F0EA] text-[#432B1F] p-2 rounded-full">{icon}</div>
      <span className="text-md">{label}</span>
    </div>
  );
}
