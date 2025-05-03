import SocialIcons from "./social-icons";

function ContactInfo() {
  return (
<div className="flex flex-col items-center lg:items-start text-center lg:text-left gap-12 w-full pt-12 lg:pt-0 lg:pl-[2.25rem] lg:gap-16">

      <div>
        <h1 className="text-3xl sm:text-4xl font-bold">Contact Us</h1>
        <p className="text-base sm:text-lg mt-2">We’d love to hear from you!</p>
      </div>

      <div className="flex flex-wrap justify-center gap-4 sm:gap-6 lg:flex-col lg:items-start">

        <ContactItem iconSrc={"/assets/images/icons/mailbox.svg"} label="info@mk.com" />
        <ContactItem iconSrc={"/assets/images/icons/phone.svg"} label="+385 123456789" />
        <ContactItem iconSrc={"/assets/images/icons/locationContact.svg"} label="address 123" />
      </div>

      <SocialIcons />
    </div>
  );
}

export default ContactInfo;

type ContactItemProps = {
  readonly iconSrc: string;
  readonly label: string;
};

function ContactItem({ iconSrc, label }: ContactItemProps) {
  return (
    <div className="flex items-center gap-2 sm:gap-4">
      <img src={iconSrc} width={"30px"} alt="icon" />
      <span className="text-md">{label}</span>
    </div>
  );
}
