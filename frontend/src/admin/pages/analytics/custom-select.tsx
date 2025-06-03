import Select from "react-select";

type OptionType = {
    value: number;
    label: string;
};

type Props = {
    options: OptionType[];
    value: number | undefined;
    onChange: (value: number | undefined) => void;
    placeholder: string;
};

export default function CustomSelect({ options, value, onChange, placeholder }: Props) {
    return (
        <Select
        options={options}
        value={options.find(opt => opt.value === value) || null}
        onChange={(option) => onChange(option?.value ?? undefined)}
        isClearable={true}
        styles={{
            menu: (provided) => ({
                ...provided,
                maxHeight: '150px',
            }),
            menuList: (provided) => ({
                ...provided,
                maxHeight: '150px',
                overflowY: 'auto',
                padding: '2px',
            }),
            option: (provided, state) => ({
                ...provided,
                paddingTop: 5,
                paddingBottom: 5,
                paddingLeft: 8,
                paddingRight: 8,
                fontSize: '16px',
                textAlign: 'center',
                backgroundColor: state.isFocused ? '#f0f0f0' : 'white',
            }),
            singleValue: (provided) => ({
                ...provided,
                textAlign: 'center',
                width: '100%',
            }),
            dropdownIndicator: (provided) => ({
                ...provided,
                padding: 2,  
                marginRight: 4, 
            }),
            clearIndicator: (provided) => ({
                ...provided,
                padding: 2,   
                marginRight: 4,
            }),
        }}
        placeholder={placeholder}
        className="text-[14px]"
        />
    );
}
