import { JSX } from "react";
import { FiMail, FiPhone, FiMapPin } from "react-icons/fi";
import SocialIcons from "./social-icons";

function ContactInfo() {
  return (
    <>
      {/* Logo + Title */}
      <div>
        <div className="mb-10">
          {/* Replace this with your actual logo */}
          <h1 className="text-4xl font-bold">Contact Us</h1>
          <p className="text-lg mt-2">We’d love to hear from you!</p>
        </div>

        {/* Info Items */}
        <div className="space-y-6">
          <ContactItem icon={<FiMail />} label="info@mk.com" />
          <ContactItem icon={<FiPhone />} label="+385 123456789" />
          <ContactItem icon={<FiMapPin />} label="address 123" />
        </div>
      </div>

      {/* Social Icons */}
      <SocialIcons />
    </>
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
      <div className="bg-[#F5F0EA] text-[#432B1F] p-2 rounded-full">
        {icon}
      </div>
      <span className="text-md">{label}</span>
    </div>
  );
}
